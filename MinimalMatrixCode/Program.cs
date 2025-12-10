
using System.Text;

class GraphReader
{
    public Graph[]? Graph6 {private set; get; }

    public class Graph(string init)
    {
        string Code = init; // исходный код
        int N;
        byte[,]? Matrix;
        bool isDiGraph = init[0] == ':' || init[0] == '&'; // sparce6 выводит графы с ":" , а как генерировать чисто digraph6 я не нашел

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
                        Console.WriteLine($"{bitPos} : {bit} : {bits}");
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
    }

    public void ReadGraphs(string filename) // функция чтения файла в объект GraphReader
    {
        List<Graph> graphs = new List<Graph>();
        foreach (string code in File.ReadAllLines(filename))
        {
            Graph graph = new Graph(code);
            graph.ParseGraph(); // парсим его матрицу, количество вершин
            graphs.Add(graph); // добавляем его
            Console.WriteLine(graph);
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
    }
}

