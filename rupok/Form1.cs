using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Steganography
{
    public partial class Form1 : Form
    {
        public enum Mode
        {
            Image,
        }

        Image image;
        byte[] file;
        string filename;
        Mode currentMode;
        Stopwatch stopwatch;

        public Form1()
        {
            InitializeComponent();
            OutputConsole.Bind(console);
            random.Checked = true;
            currentMode = Mode.Image;
            stopwatch = new Stopwatch();
        }

        private void loadImage_Click(object sender, EventArgs e)
        {
            loadDialog.FileName = "*.*";
            DialogResult res = loadDialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                if (image != null)
                {
                    image.Dispose();
                }
                string ext = Path.GetExtension(loadDialog.FileName);
                if (ext == ".png" || ext == ".bmp" || ext == ".jpg")
                {
                    try
                    {
                        image = Image.FromFile(loadDialog.FileName);
                        imageBox.Image = image;
                        OutputConsole.Write(string.Format("Image loaded \nTotal pixels = {0}", image.Width * image.Height));
                        OutputConsole.Write(string.Format("Maximum file size for this image = {0} - (file size digits + file name character count) bytes", FileSizeFormatProvider.GetFileSize((image.Width * image.Height) - 2)));
                        currentMode = Mode.Image;
                        audioLabel.Visible = false;
                    }
                    catch
                    {
                        image = null;
                        imageBox.Image = null;
                    }
                }
            }
        }

        private void encryptButton_Click(object sender, EventArgs e)
        {
            if (currentMode == Mode.Image && image != null)
            {
                Bitmap encrypted;
                if (random.Checked || randomM2.Checked)
                {
                    encrypted = Steganography.InsertEncryptedTextToImage(image, textBox.Text);
                }
                else
                {
                    encrypted = Steganography.InsertEncryptedTextToImageLinear(image, textBox.Text);
                }
                if (encrypted != null)
                {
                    saveDialog.FileName = "*.*";
                    DialogResult res = saveDialog.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        encrypted.Save(saveDialog.FileName);
                        OutputConsole.Write("Image saved");
                        MessageBox.Show("Text Encrypted");
                    }
                }
            }
        }

        private void decryptButton_Click(object sender, EventArgs e)
        {
            if (currentMode == Mode.Image && image != null)
            {
                string text;
                if (random.Checked || randomM2.Checked)
                {
                     text = Steganography.GetDecryptedTextFromImage(image);
                }
                else
                {
                    text = Steganography.GetDecryptedTextFromImageLinear(image);
                }
                if (text != null)
                {
                    decrypt.Text = text;
                    OutputConsole.Write("Text decrypted");
                    MessageBox.Show("Text Decrypted");
                }
                else
                {
                    //MessageBox.Show("This image doesn't have an encrypted text or an error occurred");
                }
            }
        }

        private void encryptFile_Click(object sender, EventArgs e)
        {
            if (currentMode == Mode.Image && image != null)
            {
                loadFileDialog.FileName = "*.*";
                DialogResult res = loadFileDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    file = File.ReadAllBytes(loadFileDialog.FileName);
                    filename = loadFileDialog.SafeFileName;
                    OutputConsole.Write("Added File to buffer");
                    Bitmap encrypted;
                    stopwatch.Restart();
                    if (random.Checked)
                    {
                        encrypted = Steganography.InsertFileToImage(image, file, filename);
                    }
                    else
                    {
                        if (linear.Checked)
                        {
                            encrypted = Steganography.InsertFileToImageLinear(image, file, filename);
                        }
                        else
                        {
                            encrypted = Steganography.InsertFileToImage2(image, file, filename);
                        }
                    }
                    if (encrypted != null)
                    {
                        stopwatch.Stop();
                        OutputConsole.Write(string.Format("Process completed in {0} ms", stopwatch.ElapsedMilliseconds));
                        saveDialog.FileName = "*.*";
                        DialogResult res2 = saveDialog.ShowDialog();
                        if (res2 == System.Windows.Forms.DialogResult.OK)
                        {
                            encrypted.Save(saveDialog.FileName);
                            OutputConsole.Write("Image saved");
                        }
                    }
                    stopwatch.Reset();
                }
            }
        }

        private void decryptFile_Click(object sender, EventArgs e)
        {
            if (currentMode == Mode.Image && image != null)
            {
                HiddenFile f;
                stopwatch.Restart();
                if (random.Checked)
                {
                    f = Steganography.GetFileFromImage(image);
                }
                else
                {
                    if (linear.Checked)
                    {
                        f = Steganography.GetFileFromImageLinear(image);
                    }
                    else
                    {
                        f = Steganography.GetFileFromImage2(image);
                    }
                }
                if (f != null)
                {
                    stopwatch.Stop();
                    OutputConsole.Write(string.Format("Process completed in {0} ms", stopwatch.ElapsedMilliseconds));
                    saveFileDialog.FileName = f.filename;
                    DialogResult res = saveFileDialog.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, f.file);
                        OutputConsole.Write("File saved");
                        if (Path.GetExtension(saveFileDialog.FileName) == ".bmp" || Path.GetExtension(saveFileDialog.FileName) == ".png" || Path.GetExtension(saveFileDialog.FileName) == ".jpg")
                        {
                            ImgPreview p = new ImgPreview(Image.FromFile(saveFileDialog.FileName));
                            p.ShowDialog();
                        }
                    }
                }
                else
                {
                    //MessageBox.Show("This image doesn't have an encrypted text or an error occurred");
                }
                stopwatch.Reset();
            }
        }

        private void console_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = console.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                MessageBox.Show(console.Items[index].ToString());
            }
        }

        private void random_CheckedChanged(object sender, EventArgs e)
        {
            if (random.Checked)
                OutputConsole.Write("Using random steganography algorithm (Requires prime number generation, slow for huge images/wav files, once the generator has reached the needed number it can process files quickly)");
            //OutputConsole.Write("Using random steganography algorithm (Not recommended for huge files or text near 60% image pixels or more than 10% of available bytes in wav files)");
        }

        private void linear_CheckedChanged(object sender, EventArgs e)
        {
            if (linear.Checked)
                OutputConsole.Write("Using linear steganography algorithm (Fastest)");
        }

        private void console_KeyDown(object sender, KeyEventArgs e)
        {
            if (console.SelectedIndex >= 0 && e.KeyCode == Keys.Delete)
            {
                console.Items.RemoveAt(console.SelectedIndex);
            }
        }

        private void loadWavFile_Click(object sender, EventArgs e)
        {
        }

        private void randomM2_CheckedChanged(object sender, EventArgs e)
        {
            //Removed
            if (randomM2.Checked)
                OutputConsole.Write("Using random method 2 steganography algorithm (Poor performance on small files, but better performance than random using bigger files) \nWorks only with files...");
        }
    }
}
