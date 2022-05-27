using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Rotate_Hexagons
{
    public partial class Form1 : Form
    {
        Bitmap myBitmap; // Объявляем битмап
        Graphics g; // Объявляем графику

        // Описываем структуру шестиугольника
        public struct T_Hexagon 
        { 
            public Point center;
            public Point[] border;
            public double[] angle;
        };
        public T_Hexagon[,] a = new T_Hexagon[110, 100]; // Объявляем массив структур шестиугольников
        public int[,] order = new int[110, 110]; // Объявляем массив порядка обхода шестиугольников для их поворота

        double r = 35.3; // Радиус описанной окружности для шестиугольника (испоьзуется для построения шестиугольников)
        Point center = new Point(15, 15); // Координаты ячейки центрального шестиугольника на плоскости 

        int cur_rotatinon_order; // Текущий слой шестиугольников для поворота 
        int cur_cnt; // Текущее кол-во пройденных итераций поворота
        int next_cnt; // Кол-во пройденных интераций поворота для следующего слоя

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Создаем шестиугольник
        void Create_Hexagon(int i, int j) 
        {
            a[i, j].border = new Point[6]; // Инициализируем массив точек границы шестиугольника
            a[i, j].angle = new double[6]; // Массив углов, соответствующих точке
            double angle = Math.PI/3; // Инициализируем начальный угол
            for (int t = 0; t < 6; t++)
            {
                // Рассчитываем координаты шестиугольника
                a[i, j].border[t].X = Convert.ToInt32(r * Math.Cos(t * angle)) + a[i, j].center.X;
                a[i, j].border[t].Y = Convert.ToInt32(r * Math.Sin(t * angle)) + a[i, j].center.Y;
                a[i, j].angle[t] = t * angle; // ... и соответствующий угол для текущей точки
            }
        }

        // Рисуем шестиугольник
        void Draw_Hexagon(int i, int j)
        {
            SolidBrush myBrush = new SolidBrush(Color.White);
            g.FillPolygon(myBrush, a[i, j].border);
        }

        // Строим все шестиугольники
        void Create_All_Hexagons(Point center)
        {
            // Обходим все шестиугольники
            for (int i = 0; i <= 2 * center.Y; i++)
            {
                for (int j = 0; j <= 2 * center.X; j++)
                {
                    // Устанавливаем сдвиг в один пиксель (для корректного отображения картинки, связано с погрешностью вычислений)
                    int shift = 0;
                    if (i > center.X) shift = 1;
                    if (i < center.Y) shift = -1;
                    
                    // Рассчитываем координаты центров всех шестиугольников (ставим их сеткой)
                    a[i, j].center.Y = Convert.ToInt32(a[center.X, center.Y].center.Y + (i - center.X) * 2 * r * Math.Sin(Math.PI / 3)) - shift;
                    a[i, j].center.X = Convert.ToInt32(a[center.X, center.Y].center.X + (j - center.Y) * 2 * r);

                    // Смещаем все шестиугольники, которые находяться от центрального шестиугольника на нечетное кол-во позиций, влево, чтобы картинка была шахматного вида
                    if (Math.Abs(i - center.Y) % 2 == 1)
                    {
                        a[i, j].center.X -= Convert.ToInt32(r);
                    }
                    // Рассчитываем координаты точек на границе шестиугольника
                    Create_Hexagon(i, j);
                }
            }
        }

        // Прорисовываем все шестиугольники
        void Draw_All_Hexagons(Point center)
        {
            // Инициализируем битмап
            myBitmap = new Bitmap(700, 700);
            g = Graphics.FromImage(myBitmap); // Инициализируем графику
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            SolidBrush myBrush = new SolidBrush(Color.Black);
            g.FillRectangle(myBrush, 0, 0, 700, 700); // Очищаем холст
            for (int i = 0; i <= 2 * center.X; i++)
            {
                for (int j = 0; j <= 2 * center.Y; j++)
                {
                    Draw_Hexagon(i, j); // Рисуем шестиугольник
                }
            }
            pictureBox1.Image = myBitmap; // Записываем содержание bitmap в picturebox
        }

        // Устанавливаем порядок обхода шестиугольников для поворота
        void Establish_Order(Point center)
        {
            for (int i = 0; i < 2 * center.X; i++)
            {
                for (int j = 0; j < 2 * center.Y; j++)
                {
                    order[i, j] = -1;
                }
            }

            // Устанавливаем порядок в соответствии с номером слоя
            for (int cur_order = 0; cur_order <= 20; cur_order++)
            {
                if (center.X - cur_order >= 0 && center.X + cur_order <= 2 * center.X)
                {
                    if (center.Y - cur_order >= 0 && center.Y + cur_order <= 2 * center.Y)
                    {
                        for (int shift = 0; shift <= cur_order; shift++)
                        {
                            int tmp_shift = shift;
                            if (shift % 2 == 1) tmp_shift++;
                            tmp_shift /= 2;
                            order[center.X - cur_order + tmp_shift, center.Y - shift] = cur_order;
                            order[center.X - cur_order + tmp_shift, center.Y + shift] = cur_order;

                            order[center.X + cur_order - shift / 2, center.Y - shift] = cur_order;
                            order[center.X + cur_order - shift / 2, center.Y + shift] = cur_order;

                        }
                        int t = cur_order;
                        if (cur_order % 2 == 1) t++;
                        t /= 2;
                        for (int cur_x = center.X - cur_order + t; cur_x <= center.X + cur_order - cur_order / 2; cur_x++)
                        {
                            order[cur_x, center.Y - cur_order] = cur_order;
                            order[cur_x, center.Y + cur_order] = cur_order;
                        }

                    }
                }
                
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Инициалируем координаты центра для центрального на плоскости шестиугольника
            a[center.X, center.Y].center = new Point(350, 350);
            // Строим по координатам центрального шестиугольника все шестиугольники
            Create_All_Hexagons(center);
            Establish_Order(center); // Устанавливаем порядок обхода для поворота
            Draw_All_Hexagons(center); // Рисуем все шестиугольники
            timer1.Start(); // Запускаем таймер (начало поворота от центрального шестиугольника)
        }

        // Поворачиваем шестиугольник
        void Rotate_Hexagon(int j, int i)
        {
            for (int t = 0; t < 6; t++)
            {
                // Координаты граничных точек шестиугольника перещитываем относительно центра шестиугольника
                a[i, j].border[t].X -= a[i, j].center.X;
                a[i, j].border[t].Y -= a[i, j].center.Y;
                // Рассчитываем угол для текущей точки
                double cur_angle = a[i, j].angle[t] + Math.PI / 180;
               
                a[i, j].angle[t] = cur_angle;
                // Рассчитываем координаты точек с учетом поворота
                a[i, j].border[t].X = Convert.ToInt32(r * Math.Cos(cur_angle));
                a[i, j].border[t].Y = Convert.ToInt32(r * Math.Sin(cur_angle));
                // Пересчитываем координаты граничных точек относительно абсолютного начала координат
                a[i, j].border[t].X += a[i, j].center.X;
                a[i, j].border[t].Y += a[i, j].center.Y;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            cur_rotatinon_order = 0; // Инициализируем номер текущего слоя
            cur_cnt = 0; // Инициализируем текущее кол-во выполненных итераций поворота (для текущего слоя)
            next_cnt = 0; // Инициализируем кол-во выполненных итераций поворота для следующего слоя

            timer2.Start(); // Запускаем таймер поворота
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            cur_cnt++; // Увеличиваем кол-во итераций поворота
            if (cur_cnt > 15) next_cnt++; // Если кол-во итераций поворота больше половины необходимого, то начинаем вращать следующий слой

            // Поворачиваем шестиугольники текущего слоя
            for (int i = 0; i <= 2 * center.X; i++)
            {
                for (int j = 0; j <= 2 * center.Y; j++)
                {
                    if (order[i, j] == cur_rotatinon_order) Rotate_Hexagon(i, j); 
                }
            }

            // Поворачиваем шестиугольники следующего слоя
            for (int i = 0; i <= 2 * center.X; i++)
            {
                for (int j = 0; j <= 2 * center.Y; j++)
                {
                    if (next_cnt != 0 && order[i, j] == cur_rotatinon_order+1) Rotate_Hexagon(i, j);
                }
            }
             
            // Рисуем все шестиугольники
            Draw_All_Hexagons(center);
            
            // Если полностью перевернули шестиугольники текущего слоя, то переходим к следующему
            if (cur_cnt == 30)
            {
                cur_rotatinon_order++;
                cur_cnt = 15;
                next_cnt = 0;
            }

            // Если мы обошли все слои, то останавливаем вращение
            if (cur_rotatinon_order == 20)
            {
                timer2.Stop();
                return;
            }
        }

        
    }
}
