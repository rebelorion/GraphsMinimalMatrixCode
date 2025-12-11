
using System.Text;
using System.Collections.Generic;


class GraphReader
{
    public Graph[]? Graph6 {private set; get; }

    public class Graph(string init)
    {
        string Code = init; // исходный код
        int N;
        byte[,]? Matrix;
        bool isDiGraph = init[0] == '&';

        public override string ToString()
        {
            string idg = isDiGraph ? "digraph6" : "graph6";
            return $"Код: {Code} ; ({idg}) ; Вершин: {N} ; Матрица: \n{GetMatrixString()}";
        }

        private string GetMatrixString()
        {
            if (Matrix == null) return "";
            StringBuilder mtx = new StringBuilder();
            int count = 0;
            foreach (byte bit in Matrix) // считываются биты сразу все, между нужными вставляется "\n"
            {
                count++;
                string bitStr = (bit == 1) ? "1 " : "0 ";
                mtx.Append(bitStr);
                if (count % N == 0) mtx.Append("\n"); // переход на новый ряд
            }
            return mtx.ToString();
        }

        public void ParseGraph()
        {
            if (!isDiGraph) // вариант graph6
            {
                N = Code[0] - 63; // получаем и записывае кол-во вершин

                // создаем матрицу byte (n * n)
                byte[,] matrix = new byte[N,N];

                int bitPos = 0;
                for (int i = 1; i < Code.Length; i++) // заполняем ее
                {
                    int symbol = Code[i] - 63; // получаем число, которое в биты будем переводить
                    string bits = Convert.ToString(symbol, 2).PadLeft(6, '0'); // поулучаем 6 битовую строку
                    foreach (char bit in bits)
                    {
                        var coord = TranslatePosition(bitPos);
                        if (coord.x >= N || coord.y >= N) continue; // пропуск лишних бит. их не много на скорость это не повлияет
                        matrix[coord.x, coord.y] = (byte)(bit == '1' ? 1 : 0); // записываем в матрицу
                        matrix[coord.y, coord.x] = (byte)(bit == '1' ? 1 : 0); // и в зеркальное от гл. диагонали положение
                        bitPos++; // увеличиваем текущую позицию
                    }
                }
                Matrix = matrix;
            }
            else
            {
                N = Code[1] - 63;

                // создаем матрицу byte (n * n)
                byte[,] matrix = new byte[N,N];

                int bitPos = 0;
                for (int i = 2; i < Code.Length; i++)
                {
                    int symbol = Code[i] - 63;
                    string bits = Convert.ToString(symbol, 2).PadLeft(6, '0'); // поулучаем 6 битовую строку
                    foreach (char bit in bits)
                    {
                        int x = bitPos / N;
                        int y = bitPos % N;
                        if (x >= N || y >= N) continue; // пропуск лишних бит. их не много на скорость это не повлияет
                        matrix[x, y] = (byte)(bit == '1' ? 1 : 0); // записываем в матрицу
                        // Console.WriteLine($"{bitPos} : {bit} : {bits}");
                        bitPos++;
                    }
                }
                Matrix = matrix;
            }
        }

        // функция, которая возвращает позицию в матрице
        private (int x, int y) TranslatePosition(int posNumber)
        {
            int i;
            for (i = 1; posNumber >= i; i++)
            {
                posNumber -= i;
            }
            return (i, posNumber);
        }
    
        public int GetMinimalCode()
        {
            int[] colors = new int[N];

            if (Matrix == null) return -1;

            int minimalVert = 0;
            int minimalDegree = N + 1;
            for (int i = 0; i < N; i++)
            {
                int sum = 0;
                for (int j = 0; j < N; j++)
                {
                    sum += Matrix[i, j];
                }
                if (sum < minimalDegree)
                {
                    minimalDegree = sum;
                    minimalVert = i;
                }
                colors[i] = sum;
            }

            // составляем массив элементов кроме вершины с минимальной
            // степенью. Их и будем переставлять
            List<int> others1 = new List<int>();
            for (int i = 0; i < N; i++)
            {
                if (i == minimalVert) continue;
                others1.Add(i);
            }
            int[] others = others1.ToArray();

            int minimalCode = int.MaxValue;
            do // на каждой итерации генерируем перестановку
            {
                int tryCode = GetCode(minimalVert, others);
                if (tryCode < minimalCode)
                {
                    minimalCode = tryCode;
                }

            } while (NextPermutation(others)); 

            return minimalCode;

            // int[] newColors = new int[N]; // по умолчанию заполнен нулями
            // while (!newColors.SequenceEqual(colors)) // поэлементное сравнение
            // {
            //     newColors = new int[N];
            //     for (int i = 0; i < N; i++)
            //     {
            //         int hash = colors[i];
            //         int exponent = 1;
            //         for (int j = 0; j < N; j++)
            //         {
            //             if (i == j) continue;
            //             exponent *= N;
            //             hash += colors[j] * exponent;
            //         }
            //         newColors[i] = hash;
            //     }
            //     colors = (int[])newColors.Clone();
            // }

            // Console.WriteLine();
            // foreach (int color in colors)
            // {
            //     Console.Write($"{color} | ");
            // }

            // получили разбиение. 
            // теперь можем делать перестановки этих разбиений
            
        }

        // вычисляет код на основе перестановки
        int GetCode(int lead, int[] permutation)
        {
            if (Matrix == null) return 0;

            int result = 0;
            int exponent = 1;
            for (int i = N-1; i >= 0; i--)
            {
                for (int j = N-1; j > i; j--)
                {
                    // получаем число в матрице (0 или 1)
                    // j - 1 так как у нас массив permutations не содержит lead вершину
                    int multiplier = i != 0 ? 
                        Matrix[permutation[i - 1], permutation[j - 1]] : // то есть если мы смотрим чисто по массиву permutation
                        Matrix[lead, permutation[j - 1]]; // или смотрим верхнюю строку, которая lead вершина
                    result += multiplier * exponent;
                    exponent *= 2;
                }
            }
            return result;
        }

        static bool NextPermutation(int[] arr)
        {
            int i = arr.Length - 2;
            while (i >= 0 && arr[i] >= arr[i + 1]) i--;
            if (i < 0) return false;
            
            int j = arr.Length - 1;
            while (arr[j] <= arr[i]) j--;
            
            Swap(arr, i, j);
            Reverse(arr, i + 1, arr.Length - 1);
            return true;
        }
        
        static void Swap(int[] arr, int i, int j) => (arr[j], arr[i]) = (arr[i], arr[j]);
        
        static void Reverse(int[] arr, int start, int end)
        {
            while (start < end) Swap(arr, start++, end--);
        }
        
    }

    public void ReadGraphs(string filename) // функция чтения файла в объект GraphReader
    {
        List<Graph> graphs = new List<Graph>();
        foreach (string code in File.ReadAllLines(filename))
        {
            Graph graph = new Graph(code);
            graph.ParseGraph(); // парсим его матрицу, количество вершин
            graphs.Add(graph); // добавляем его
            // Console.WriteLine(graph);
        }
        Graph6 = graphs.ToArray();
    }

} 

class Program
{
    static void Main(string[] args)
    {
        string filename = args[0]; // файл - первый и единственный аргумент

        GraphReader graphReader = new GraphReader(); // инициализируем
        graphReader.ReadGraphs(filename); // считываем графы

        int minimalCode = graphReader.Graph6![1].GetMinimalCode();
        Console.WriteLine(graphReader.Graph6[1]);

        Console.WriteLine(minimalCode);
    }
}

