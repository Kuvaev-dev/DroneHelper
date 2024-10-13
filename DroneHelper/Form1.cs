using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DroneHelper
{
    public partial class Form1 : Form
    {
        // Генератор випадкових чисел
        private static readonly Random _rnd = new Random();

        // Константи для налаштування алгоритму
        const int GenerationSize = 100; // Розмір покоління
        const int GenerationNumbers = 200; // Кількість поколінь
        const double MutationProbability = 0.2; // Імовірність мутації
        const int NumberOfDrones = 10; // Кількість дронів
        static double Radius; // Радіус дії дронів

        // Список ворогів, де кожен ворог представлений координатами (x, y)
        static readonly List<(double, double)> enemies = new List<(double, double)>();

        public Form1()
        {
            InitializeComponent();
            // Генерація випадкової кількості ворогів при запуску форми
            GenerateEnemies(_rnd.Next(30, 100));

            // Встановлення фіксованого розміру форми
            FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        // Обробник події для кнопки "Почати"
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) // Перевірка введених даних
                return;

            Radius = Convert.ToDouble(txtRadius.Text); // Зчитування радіуса з текстового поля
            List<KeyValuePair<int[], double>> generation = GenerateRandom(); // Генерація початкового покоління
            SortGeneration(generation); // Сортування покоління за пристосованістю

            // Генерація нових поколінь
            for (int getNum = 0; getNum < GenerationNumbers; getNum++)
            {
                generation = GenerateNewGeneration(generation, true);
                SortGeneration(generation);
            }

            int[] bestGenome = generation[0].Key; // Отримання найкращого геному
            DrawDrones(bestGenome, pictureBoxAfter); // Відображення дронів на карті
        }

        // Обробник події для кнопки "Згенерувати ворогів"
        private void btnGenerateEnemies_Click(object sender, EventArgs e)
        {
            GenerateEnemies(_rnd.Next(30, 100)); // Генерація нових ворогів
        }

        // Обробник події для кнопки "Зберегти мапу"
        private void btnSaveMap_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png";
                sfd.Title = "Зберегти мапу";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Збереження зображення карти у файл
                    Bitmap bmp = new Bitmap(pictureBoxAfter.Width, pictureBoxAfter.Height);
                    pictureBoxAfter.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    bmp.Save(sfd.FileName);

                    // Збереження координат дронів у текстовий файл
                    string textFileName = System.IO.Path.ChangeExtension(sfd.FileName, ".txt");
                    using (StreamWriter sw = new StreamWriter(textFileName))
                    {
                        foreach (var item in lstDroneCoordinates.Items)
                        {
                            sw.WriteLine(item.ToString());
                        }
                    }
                }
            }
        }

        // Перевірка введених даних
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtRadius.Text))
            {
                MessageBox.Show("Будь ласка, введіть радіус.", "Помилка вводу", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!double.TryParse(txtRadius.Text, out double radius) || radius <= 0)
            {
                MessageBox.Show("Будь ласка, введіть дійсне додатне число для радіуса.", "Помилка вводу", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        // Генерація випадкових ворогів
        // Параметр 100.0 можна змінити для більшої розсіяності ворогів
        private void GenerateEnemies(int count)
        {
            enemies.Clear(); // Очищення списку ворогів
            for (int i = 0; i < count; i++)
            {
                double x = _rnd.NextDouble() * 100.0; // Випадкова координата X
                double y = _rnd.NextDouble() * 100.0; // Випадкова координата Y
                enemies.Add((x, y)); // Додавання ворога до списку
            }
            DrawEnemies(pictureBoxBefore); // Відображення ворогів на карті
        }

        // Відображення ворогів на карті
        private void DrawEnemies(PictureBox pictureBox)
        {
            Bitmap bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White); // Очищення зображення
                foreach (var enemy in enemies)
                {
                    g.FillEllipse(Brushes.Red, (float)enemy.Item1, (float)enemy.Item2, 5, 5); // Малювання ворога
                }
            }
            pictureBox.Image = bmp; // Встановлення зображення на PictureBox
        }

        // Відображення дронів на карті
        private void DrawDrones(int[] bestGenome, PictureBox pictureBox)
        {
            Bitmap bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White); // Очищення зображення
                foreach (var enemy in enemies)
                {
                    g.FillEllipse(Brushes.Red, (float)enemy.Item1, (float)enemy.Item2, 5, 5); // Малювання ворогів }

                    lstDroneCoordinates.Items.Clear(); // Очищення списку координат дронів
                    for (int i = 0; i < NumberOfDrones; i++)
                    {
                        double x = GetX(bestGenome[i]); // Отримання координати X дрона
                        double y = GetY(bestGenome[i]); // Отримання координати Y дрона
                        g.FillEllipse(Brushes.Blue, (float)x, (float)y, 5, 5); // Малювання дрона
                        g.DrawEllipse(Pens.Blue, (float)(x - Radius), (float)(y - Radius), (float)(Radius * 2), (float)(Radius * 2)); // Малювання радіуса дії дрона

                        lstDroneCoordinates.Items.Add($"Дрон {i + 1}: X = {x}, Y = {y}"); // Додавання координат дрона до списку
                    }
                }
                pictureBox.Image = bmp; // Встановлення зображення на PictureBox
            }
        }

        // Генерація випадкових геномів для початкового покоління
        private List<KeyValuePair<int[], double>> GenerateRandom()
        {
            List<KeyValuePair<int[], double>> result = new List<KeyValuePair<int[], double>>();
            Parallel.For(0, GenerationSize, i =>
            {
                int[] genome = new int[NumberOfDrones];
                for (int j = 0; j < NumberOfDrones; j++)
                {
                    genome[j] = _rnd.Next(); // Генерація випадкового геному
                }
                double fitness = Fitness(genome);
                lock (result) // Забезпечення потокобезпечності при додаванні до списку {
                    result.Add(new KeyValuePair<int[], double>(genome, fitness));
            });
            return result;
        }

        // Сортування покоління за пристосованістю
        private void SortGeneration(List<KeyValuePair<int[], double>> generation)
        {
            generation.Sort((x, y) => y.Value.CompareTo(x.Value)); // Сортування за спаданням пристосованості
            if (generation.Count > GenerationSize)
                generation.RemoveRange(GenerationSize, generation.Count - GenerationSize); // Видалення зайвих елементів
        }

        // Обчислення координати Y з геному
        // Параметр 100 можна змінити для більшої розсіяності ворогів
        private double GetY(int genome)
        {
            int y = genome & 0xffff;
            return y * 100.0 / 0x10000;
        }

        // Обчислення координати X з геному
        // Параметр 100.0 можна змінити для більшої розсіяності ворогів
        private double GetX(int genome)
        {
            int x = (genome >> 16) & 0xffff;
            return x * 100.0 / 0x10000;
        }

        // Обчислення пристосованості геному
        private double Fitness(int[] genome)
        {
            HashSet<int> destroyedEnemies = new HashSet<int>();

            for (int i = 0; i < NumberOfDrones; i++)
            {
                double droneX = GetX(genome[i]);
                double droneY = GetY(genome[i]);

                for (int j = 0; j < enemies.Count; j++)
                {
                    double enemyX = enemies[j].Item1;
                    double enemyY = enemies[j].Item2;

                    if (Math.Sqrt(Math.Pow(droneX - enemyX, 2) + Math.Pow(droneY - enemyY, 2)) <= Radius)
                    {
                        destroyedEnemies.Add(j); // Додавання індексу знищеного ворога
                    }
                }
            }

            return destroyedEnemies.Count; // Кількість знищених ворогів як значення пристосованості
        }

        // Генерація нового покоління з батьків
        private List<KeyValuePair<int[], double>> GenerateNewGeneration(List<KeyValuePair<int[], double>> parents, bool useElitism)
        {
            List<KeyValuePair<int[], double>> result = new List<KeyValuePair<int[], double>>();

            if (useElitism)
                result.Add(parents[0]); // Додавання найкращого геному з попереднього покоління

            Parallel.For(result.Count, GenerationSize, _ =>
            {
                int parent1 = _rnd.Next(GenerationSize);
                int parent2 = _rnd.Next(GenerationSize);

                int[] child = new int[NumberOfDrones];
                for (int i = 0; i < NumberOfDrones; i++)
                {
                    int mask = ~0 << _rnd.Next(32);
                    child[i] = parents[parent1].Key[i] & mask | parents[parent2].Key[i] & ~mask; // Кросовер генів

                    if (_rnd.NextDouble() < MutationProbability)
                        child[i] ^= 1 << _rnd.Next(32); // Мутація гену
                }

                double fitness = Fitness(child);
                lock (result) // Забезпечення потокобезпечності при додаванні до списку
                {
                    result.Add(new KeyValuePair<int[], double>(child, fitness));
                }
            });

            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}