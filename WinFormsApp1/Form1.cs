using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private string currentFilePath = "";
        private TabControl tabControl;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel lineColumnLabel;
        private ToolStripStatusLabel languageLabel;
        private System.Windows.Forms.Timer statusTimer;

        private Panel lineNumbersPanel;
        private bool isUpdatingLineNumbers = false;

        private Dictionary<TabPage, string> filePaths = new Dictionary<TabPage, string>();

        private bool isRussianLanguage = true;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready");
            lineColumnLabel = new ToolStripStatusLabel("Ln: 1, Col: 1");
            languageLabel = new ToolStripStatusLabel("Language: Russian");

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel("|"));
            statusStrip.Items.Add(lineColumnLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel("|"));
            statusStrip.Items.Add(languageLabel);

            this.Controls.Add(statusStrip);
            statusStrip.Dock = DockStyle.Bottom;

            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 100;
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            CreateTabControl();

            AddMenuItems();
        }

        private void AddMenuItems()
        {
            ToolStripMenuItem sizeMenuItem = new ToolStripMenuItem("Размер текста");
            sizeMenuItem.DropDownItems.Add("Мелкий (10pt)", null, (s, ev) => ChangeFontSize(10));
            sizeMenuItem.DropDownItems.Add("Средний (12pt)", null, (s, ev) => ChangeFontSize(12));
            sizeMenuItem.DropDownItems.Add("Крупный (14pt)", null, (s, ev) => ChangeFontSize(14));
            sizeMenuItem.DropDownItems.Add("Очень крупный (16pt)", null, (s, ev) => ChangeFontSize(16));
            Edit.DropDownItems.Add(sizeMenuItem);

            ToolStripMenuItem languageMenuItem = new ToolStripMenuItem("Язык");
            languageMenuItem.DropDownItems.Add("Русский", null, (s, ev) => SetRussianLanguage());
            languageMenuItem.DropDownItems.Add("English", null, (s, ev) => SetEnglishLanguage());
            Edit.DropDownItems.Add(languageMenuItem);
        }

        private void CreateTabControl()
        {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            tableLayoutPanel1.Controls.Clear();

            string tabName = isRussianLanguage ? "Документ 1" : "Document 1";
            TabPage firstTab = new TabPage(tabName);

            Panel textContainer = new Panel();
            textContainer.Dock = DockStyle.Fill;

            Panel tabLineNumbers = new Panel();
            tabLineNumbers.BackColor = Color.LightGray;
            tabLineNumbers.Width = 40;
            tabLineNumbers.Paint += (s, e) => LineNumbersPanel_Paint(s, e, textBox1);
            tabLineNumbers.Dock = DockStyle.Left;

            textBox1.Dock = DockStyle.Fill;

            textContainer.Controls.Add(textBox1);
            textContainer.Controls.Add(tabLineNumbers);

            firstTab.Controls.Add(textContainer);
            tabControl.TabPages.Add(firstTab);

            filePaths[firstTab] = "";

            tableLayoutPanel1.Controls.Add(tabControl, 0, 0);
            tabControl.Dock = DockStyle.Fill;
        }

        private void LineNumbersPanel_Paint(object sender, PaintEventArgs e, RichTextBox targetTextBox)
        {
            if (isUpdatingLineNumbers) return;

            RichTextBox currentTextBox = targetTextBox ?? GetCurrentRichTextBox();
            if (currentTextBox == null) return;

            isUpdatingLineNumbers = true;

            e.Graphics.Clear(Color.LightGray);

            int firstIndex = currentTextBox.GetCharIndexFromPosition(new Point(0, 0));
            int firstLine = currentTextBox.GetLineFromCharIndex(firstIndex);

            int height = currentTextBox.Height;
            int lineHeight = currentTextBox.Font.Height;
            int visibleLines = height / lineHeight;

            for (int i = 0; i <= visibleLines + 1; i++)
            {
                int lineNumber = firstLine + i + 1;
                if (lineNumber > currentTextBox.Lines.Length) break;

                Point linePos = currentTextBox.GetPositionFromCharIndex(
                    currentTextBox.GetFirstCharIndexFromLine(firstLine + i));

                if (linePos.Y >= 0 && linePos.Y < height)
                {
                    e.Graphics.DrawString(lineNumber.ToString(),
                        currentTextBox.Font,
                        Brushes.Black,
                        5,
                        linePos.Y);
                }
            }

            isUpdatingLineNumbers = false;
        }

        private RichTextBox GetCurrentRichTextBox()
        {
            if (tabControl.SelectedTab != null && tabControl.SelectedTab.Controls.Count > 0)
            {
                Panel container = tabControl.SelectedTab.Controls[0] as Panel;
                if (container != null && container.Controls.Count > 0)
                {
                    return container.Controls[0] as RichTextBox;
                }
            }
            return null;
        }

        private string GetCurrentFilePath()
        {
            if (tabControl.SelectedTab != null && filePaths.ContainsKey(tabControl.SelectedTab))
            {
                return filePaths[tabControl.SelectedTab];
            }
            return "";
        }

        private void SetCurrentFilePath(string path)
        {
            if (tabControl.SelectedTab != null)
            {
                filePaths[tabControl.SelectedTab] = path;
                if (!string.IsNullOrEmpty(path))
                {
                    tabControl.SelectedTab.Text = Path.GetFileName(path);
                }
            }
        }

        private void TextBox1_Scroll(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                Panel container = tabControl.SelectedTab.Controls[0] as Panel;
                if (container != null && container.Controls.Count > 1)
                {
                    container.Controls[1].Invalidate();
                }
            }
        }

        private void TextBox1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateCursorPosition();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab != null)
            {
                Panel container = tabControl.SelectedTab.Controls[0] as Panel;
                if (container != null && container.Controls.Count > 1)
                {
                    container.Controls[1].Invalidate();
                }
            }
        }

        private void UpdateCursorPosition()
        {
            RichTextBox currentTextBox = GetCurrentRichTextBox();
            if (currentTextBox != null)
            {
                int index = currentTextBox.SelectionStart;
                int line = currentTextBox.GetLineFromCharIndex(index);
                int column = index - currentTextBox.GetFirstCharIndexFromLine(line);

                if (isRussianLanguage)
                {
                    lineColumnLabel.Text = $"Стр: {line + 1}, Стб: {column + 1}";
                }
                else
                {
                    lineColumnLabel.Text = $"Ln: {line + 1}, Col: {column + 1}";
                }
            }
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            RichTextBox currentTextBox = GetCurrentRichTextBox();
            if (currentTextBox != null)
            {
                int charCount = currentTextBox.TextLength;
                int lineCount = currentTextBox.Lines.Length;
                string currentPath = GetCurrentFilePath();

                if (string.IsNullOrEmpty(currentPath))
                {
                    if (isRussianLanguage)
                    {
                        statusLabel.Text = $"Новый документ | Символов: {charCount} | Строк: {lineCount}";
                    }
                    else
                    {
                        statusLabel.Text = $"New document | Characters: {charCount} | Lines: {lineCount}";
                    }
                }
                else
                {
                    FileInfo fileInfo = new FileInfo(currentPath);
                    if (isRussianLanguage)
                    {
                        statusLabel.Text = $"Файл: {Path.GetFileName(currentPath)} | Размер: {fileInfo.Length} байт | Символов: {charCount}";
                    }
                    else
                    {
                        statusLabel.Text = $"File: {Path.GetFileName(currentPath)} | Size: {fileInfo.Length} bytes | Characters: {charCount}";
                    }
                }
            }

            if (tabControl.SelectedTab != null)
            {
                Panel container = tabControl.SelectedTab.Controls[0] as Panel;
                if (container != null && container.Controls.Count > 1)
                {
                    container.Controls[1].Invalidate();
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                OpenFileInNewTab(files[0]);
            }
        }

        private void OpenFileInNewTab(string filePath)
        {
            try
            {
                string fileContent = System.IO.File.ReadAllText(filePath);

                string tabName = isRussianLanguage ? Path.GetFileName(filePath) : Path.GetFileName(filePath);
                TabPage tabPage = new TabPage(tabName);

                Panel textContainer = new Panel();
                textContainer.Dock = DockStyle.Fill;

                Panel tabLineNumbers = new Panel();
                tabLineNumbers.BackColor = Color.LightGray;
                tabLineNumbers.Width = 40;
                tabLineNumbers.Dock = DockStyle.Left;

                RichTextBox richTextBox = new RichTextBox();
                richTextBox.Dock = DockStyle.Fill;
                richTextBox.Text = fileContent;
                richTextBox.TextChanged += TextBox1_TextChanged;
                richTextBox.SelectionChanged += TextBox1_SelectionChanged;
                richTextBox.VScroll += TextBox1_Scroll;

                tabLineNumbers.Paint += (s, ev) => LineNumbersPanel_Paint(s, ev, richTextBox);

                textContainer.Controls.Add(richTextBox);
                textContainer.Controls.Add(tabLineNumbers);

                tabPage.Controls.Add(textContainer);
                tabControl.TabPages.Add(tabPage);
                tabControl.SelectedTab = tabPage;

                filePaths[tabPage] = filePath;
            }
            catch (Exception ex)
            {
                string errorMessage = isRussianLanguage ? "Ошибка при открытии файла: " : "Error opening file: ";
                string errorTitle = isRussianLanguage ? "Ошибка" : "Error";

                MessageBox.Show($"{errorMessage}{ex.Message}",
                    errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCursorPosition();

            currentFilePath = GetCurrentFilePath();
        }

        private void ChangeFontSize(float newSize)
        {
            RichTextBox currentTextBox = GetCurrentRichTextBox();
            if (currentTextBox != null)
            {
                currentTextBox.Font = new Font(currentTextBox.Font.FontFamily, newSize);
            }
            textBox2.Font = new Font(textBox2.Font.FontFamily, newSize);
        }

        private void SetRussianLanguage()
        {
            isRussianLanguage = true;

            File.Text = "Файл";
            Create.Text = "Создать";
            Open.Text = "Открыть";
            Save.Text = "Сохранить";
            Save_as.Text = "Сохранить как";
            Exit.Text = "Выход";

            Edit.Text = "Правка";
            Undo.Text = "Отменить";
            Redo.Text = "Повторить";
            Cut.Text = "Вырезать";
            Copy.Text = "Копировать";
            Paste.Text = "Вставить";
            Delete.Text = "Удалить";
            Select_all.Text = "Выделить всё";

            foreach (ToolStripMenuItem item in Edit.DropDownItems)
            {
                if (item.Text == "Text size" || item.Text == "Размер текста")
                {
                    item.Text = "Размер текста";
                    foreach (ToolStripMenuItem subItem in item.DropDownItems)
                    {
                        if (subItem.Text.Contains("Small")) subItem.Text = "Мелкий (10pt)";
                        else if (subItem.Text.Contains("Medium")) subItem.Text = "Средний (12pt)";
                        else if (subItem.Text.Contains("Large")) subItem.Text = "Крупный (14pt)";
                        else if (subItem.Text.Contains("Extra Large")) subItem.Text = "Очень крупный (16pt)";
                    }
                }
                if (item.Text == "Language" || item.Text == "Язык")
                {
                    item.Text = "Язык";
                }
            }

            Help_me.Text = "Справка";
            Call_help.Text = "Вызов справки";
            About.Text = "О программе";

            toolStripButton1.Text = "Создать";
            toolStripButton2.Text = "Открыть";
            toolStripButton3.Text = "Сохранить";
            toolStripButton4.Text = "Отменить";
            toolStripButton5.Text = "Повторить";
            toolStripButton6.Text = "Копировать";
            toolStripButton7.Text = "Вырезать";
            toolStripButton8.Text = "Вставить";

            languageLabel.Text = "Язык: Русский";
            statusLabel.Text = "Готов к работе";

            UpdateTabTitles();

            UpdateCursorPosition();
        }

        private void SetEnglishLanguage()
        {
            isRussianLanguage = false;

            File.Text = "File";
            Create.Text = "New";
            Open.Text = "Open";
            Save.Text = "Save";
            Save_as.Text = "Save As";
            Exit.Text = "Exit";

            Edit.Text = "Edit";
            Undo.Text = "Undo";
            Redo.Text = "Redo";
            Cut.Text = "Cut";
            Copy.Text = "Copy";
            Paste.Text = "Paste";
            Delete.Text = "Delete";
            Select_all.Text = "Select All";

            foreach (ToolStripMenuItem item in Edit.DropDownItems)
            {
                if (item.Text == "Размер текста" || item.Text == "Text size")
                {
                    item.Text = "Text size";
                    foreach (ToolStripMenuItem subItem in item.DropDownItems)
                    {
                        if (subItem.Text.Contains("Мелкий")) subItem.Text = "Small (10pt)";
                        else if (subItem.Text.Contains("Средний")) subItem.Text = "Medium (12pt)";
                        else if (subItem.Text.Contains("Крупный")) subItem.Text = "Large (14pt)";
                        else if (subItem.Text.Contains("Очень крупный")) subItem.Text = "Extra Large (16pt)";
                    }
                }
                if (item.Text == "Язык" || item.Text == "Language")
                {
                    item.Text = "Language";
                }
            }

            Help_me.Text = "Help";
            Call_help.Text = "Call help";
            About.Text = "About";

            toolStripButton1.Text = "New";
            toolStripButton2.Text = "Open";
            toolStripButton3.Text = "Save";
            toolStripButton4.Text = "Undo";
            toolStripButton5.Text = "Redo";
            toolStripButton6.Text = "Copy";
            toolStripButton7.Text = "Cut";
            toolStripButton8.Text = "Paste";

            languageLabel.Text = "Language: English";
            statusLabel.Text = "Ready";

            UpdateTabTitles();

            UpdateCursorPosition();
        }

        private void UpdateTabTitles()
        {
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                TabPage tab = tabControl.TabPages[i];

                if (filePaths.ContainsKey(tab) && !string.IsNullOrEmpty(filePaths[tab]))
                {
                    tab.Text = Path.GetFileName(filePaths[tab]);
                }
                else
                {
                    if (isRussianLanguage)
                    {
                        tab.Text = $"Документ {i + 1}";
                    }
                    else
                    {
                        tab.Text = $"Document {i + 1}";
                    }
                }
            }
        }

        private void Click_button_open(object sender, EventArgs e)
        {
            string title = isRussianLanguage ? "Открыть файл" : "Open file";
            string filter = isRussianLanguage ? "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*" : "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = title;
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 1;
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    OpenFileInNewTab(openFileDialog.FileName);
                }
            }
        }

        private void Click_button_create(object sender, EventArgs e)
        {
            int tabNumber = tabControl.TabPages.Count + 1;
            string tabName = isRussianLanguage ? $"Документ {tabNumber}" : $"Document {tabNumber}";
            TabPage tabPage = new TabPage(tabName);

            Panel textContainer = new Panel();
            textContainer.Dock = DockStyle.Fill;

            Panel tabLineNumbers = new Panel();
            tabLineNumbers.BackColor = Color.LightGray;
            tabLineNumbers.Width = 40;
            tabLineNumbers.Dock = DockStyle.Left;

            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            richTextBox.TextChanged += TextBox1_TextChanged;
            richTextBox.SelectionChanged += TextBox1_SelectionChanged;
            richTextBox.VScroll += TextBox1_Scroll;

            tabLineNumbers.Paint += (s, ev) => LineNumbersPanel_Paint(s, ev, richTextBox);

            textContainer.Controls.Add(richTextBox);
            textContainer.Controls.Add(tabLineNumbers);

            tabPage.Controls.Add(textContainer);
            tabControl.TabPages.Add(tabPage);
            tabControl.SelectedTab = tabPage;

            filePaths[tabPage] = "";
        }

        private void Click_burron_save(object sender, EventArgs e)
        {
            RichTextBox currentTextBox = GetCurrentRichTextBox();
            if (currentTextBox == null) return;

            string currentPath = GetCurrentFilePath();

            if (string.IsNullOrEmpty(currentPath))
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    string filter = isRussianLanguage ? "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*" : "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    saveFileDialog.Filter = filter;
                    saveFileDialog.DefaultExt = "txt";
                    saveFileDialog.FileName = "MyFile.txt";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        currentPath = saveFileDialog.FileName;
                        SetCurrentFilePath(currentPath);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            try
            {
                System.IO.File.WriteAllText(currentPath, currentTextBox.Text);
            }
            catch (Exception ex)
            {
                string errorMessage = isRussianLanguage ? "Ошибка при сохранении файла: " : "Error saving file: ";
                string errorTitle = isRussianLanguage ? "Ошибка" : "Error";
                MessageBox.Show($"{errorMessage}{ex.Message}",
                    errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Click_button_save_as(object sender, EventArgs e)
        {
            RichTextBox currentTextBox = GetCurrentRichTextBox();
            if (currentTextBox == null) return;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                string title = isRussianLanguage ? "Сохранить файл как" : "Save file as";
                string filter = isRussianLanguage ? "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*" : "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                saveFileDialog.Title = title;
                saveFileDialog.Filter = filter;
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.FileName = "MyFile.txt";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = saveFileDialog.FileName;
                        string content = currentTextBox.Text;

                        System.IO.File.WriteAllText(filePath, content);
                        SetCurrentFilePath(filePath);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = isRussianLanguage ? "Ошибка: " : "Error: ";
                        string errorTitle = isRussianLanguage ? "Ошибка" : "Error";
                        MessageBox.Show($"{errorMessage}{ex.Message}", errorTitle,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Help(object sender, EventArgs e)
        {
            if (isRussianLanguage)
            {
                MessageBox.Show($"СПРАВКА ПО ПРОГРАММЕ\n" +
                    "МЕНЮ ФАЙЛ\n" +
                    "Создать - создает новый документ\n" +
                    "Открыть - открывает существующий файл\n" +
                    "Сохранить - сохраняет текущий документ\n" +
                    "Сохранить как - сохраняет документ под новым именем\n" +
                    "Выход - завершает работу программы\n\n" +
                    "МЕНЮ ПРАВКА\n" +
                    "Отменить - отменяет последнее действие\n" +
                    "Повторить - повторяет отмененное действие\n" +
                    "Вырезать - вырезает выделенный текст\n" +
                    "Копировать - копирует выделенный текст\n" +
                    "Вставить - вставляет текст из буфера\n" +
                    "Удалить - удаляет выделенный текст\n" +
                    "Выделить все - выделяет весь текст\n" +
                    "Размер текста - изменение размера шрифта\n" +
                    "Язык - переключение языка интерфейса\n\n" +
                    "МЕНЮ СПРАВКА\n" +
                    "Вызов справки - открывает это окно\n" +
                    "О программе - информация о программе\n\n" +
                    "ПАНЕЛЬ ИНСТРУМЕНТОВ\n" +
                    "Содержит кнопки быстрого доступа к основным функциям.\n\n" +
                    "ДОПОЛНИТЕЛЬНЫЕ ВОЗМОЖНОСТИ\n" +
                    "Вкладки - работа с несколькими документами\n" +
                    "Нумерация строк - отображается слева от текста\n" +
                    "Строка состояния - информация о текущем документе\n" +
                    "Drag-and-Drop - перетаскивание файлов в окно",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"PROGRAM HELP\n" +
                    "FILE MENU\n" +
                    "New - creates a new document\n" +
                    "Open - opens an existing file\n" +
                    "Save - saves the current document\n" +
                    "Save As - saves the document with a new name\n" +
                    "Exit - exits the program\n\n" +
                    "EDIT MENU\n" +
                    "Undo - undoes the last action\n" +
                    "Redo - redoes the undone action\n" +
                    "Cut - cuts the selected text\n" +
                    "Copy - copies the selected text\n" +
                    "aste - pastes text from the clipboard\n" +
                    "Delete - deletes the selected text\n" +
                    "Select All - selects all text\n" +
                    "Text size - changes the font size\n" +
                    "Language - switches the interface language\n\n" +
                    "HELP MENU\n" +
                    "Call help - opens this window\n" +
                    "About - information about the program\n\n" +
                    "TOOLBAR\n" +
                    "Contains quick access buttons for main functions.\n\n" +
                    "ADDITIONAL FEATURES\n" +
                    "Tabs - work with multiple documents\n" +
                    "Line numbering - displayed to the left of the text\n" +
                    "Status bar - information about the current document\n" +
                    "Drag-and-Drop - drag files into the window",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void About_proga(object sender, EventArgs e)
        {
            if (isRussianLanguage)
            {
                MessageBox.Show($"Простой текстовый редактор с функциями языкового процессора.\n" +
                          "Разработан в рамках лабораторной работы по дисциплине\n" +
                          "'Языки и методы программирования'.\n" +
                          "Выполнила Попова Дарья АВТ-314\n\n",
                    "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Simple text editor with language processor functions.\n" +
                          "Developed as part of laboratory work in the discipline\n" +
                          "'Languages and Programming Methods'.\n" +
                          "Created by Popova Daria AVT-314\n\n",
                    "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void Button_exit(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
