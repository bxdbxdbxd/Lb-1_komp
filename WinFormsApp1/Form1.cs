using System.Diagnostics;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private string currentFilePath = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Click_button_open(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Файл для открытия";
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {

                        string fileContent = File.ReadAllText(openFileDialog.FileName);
                        string filePath = openFileDialog.FileName;

                        textBox1.Text = fileContent;

                        currentFilePath = filePath;
                        MessageBox.Show($"Файл успешно загружен:\n{filePath}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Click_button_create(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Сохранение файла";
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.FileName = "MyFile.txt";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = saveFileDialog.FileName;
                        string content = "";
                        textBox1.Text = content;

                        File.WriteAllText(filePath, content);
                        currentFilePath = filePath;

                        MessageBox.Show($"Файл создан\nПуть: {filePath}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Click_burron_save(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                    saveFileDialog.DefaultExt = "txt";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        currentFilePath = saveFileDialog.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            try
            {
                File.WriteAllText(currentFilePath, textBox1.Text);
                MessageBox.Show("Файл успешно сохранен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Undo_Click(object sender, EventArgs e)
        {
            if (textBox1.CanUndo)
            {
                textBox1.Undo();
            }
        }

        private void Redo_Click(object sender, EventArgs e)
        {
            if (textBox1.CanRedo)
            {
                textBox1.Redo();
            }
        }

        private void Click_button_save_as(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Сохранение файла";
                saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.FileName = "MyFile.txt";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = saveFileDialog.FileName;
                        string content = textBox1.Text;

                        File.WriteAllText(filePath, content);
                        currentFilePath = filePath;

                        MessageBox.Show($"Файл сохранен\nПуть: {filePath}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void Cut_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText.Length > 0)
            {
                textBox1.Cut();
            }
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText.Length > 0)
            {
                textBox1.Copy();
            }
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                textBox1.Paste();
            }
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            if (textBox1.SelectedText.Length > 0)
            {
                int selectionStart = textBox1.SelectionStart;
                int selectionLength = textBox1.SelectionLength;

                textBox1.Text = textBox1.Text.Remove(selectionStart, selectionLength);

                textBox1.SelectionStart = selectionStart;
            }
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void Help(object sender, EventArgs e)
        {
            MessageBox.Show($"СПРАВКА ПО ПРОГРАММЕ\n" +
                
                "МЕНЮ ФАЙЛ\n" +
                "• Создать - создает новый документ\n" +
                "• Открыть - открывает существующий файл\n" +
                "• Сохранить - сохраняет текущий документ\n" +
                "• Сохранить как - сохраняет документ под новым именем\n" +
                "• Выход - завершает работу программы\n\n" +
                
                "МЕНЮ ПРАВКА\n" +
                "• Отменить - отменяет последнее действие\n" +
                "• Повторить - повторяет отмененное действие\n" +
                "• Вырезать - вырезает выделенный текст\n" +
                "• Копировать - копирует выделенный текст\n" +
                "• Вставить - вставляет текст из буфера\n" +
                "• Удалить - удаляет выделенный текст\n" +
                "• Выделить все - выделяет весь текст\n\n" +
                
                "МЕНЮ СПРАВКА\n" +
                "• Вызов справки - открывает это окно\n" +
                "• О программе - информация о программе\n\n" +
                
                "ПАНЕЛЬ ИНСТРУМЕНТОВ\n" +
                "Содержит кнопки быстрого доступа к основным функциям.\n\n",
                "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void About_proga(object sender, EventArgs e)
        {
            MessageBox.Show($"Простой текстовый редактор с функциями языкового процессора.\n" +
                      "Разработан в рамках лабораторной работы по дисциплине\n" +
                      "'Языки и методы программирования'.\n" + 
                      "Выполнила Попова Дарья АВТ-314",
                "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
