namespace Rogue;

public enum GameObject
{
    Floor,
    Wall,
    Enemy,
    Trap,
    Life,
    Corpse,
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

public class Game
{
    private GameObject[,] board;
    private Random random = new Random();

    public Game(int width, int height)
    {
        this.board = new GameObject[width, height];
        this.FillBoard();

        this.PlayerRow = 1;
        this.PlayerCol = 1;
        this.Lives = 5;
    }

    private void FillBoard()
    {
        // Poner todas las paredes horizontales
        for (int col = 0; col < this.board.GetLength(0); col++)
        {
            this.board[col, 0] = GameObject.Wall;
            this.board[col, this.board.GetLength(1) - 1] = GameObject.Wall;
        }

        // Poner todas las paredes verticales
        for (int row = 0; row < this.board.GetLength(1); row++)
        {
            this.board[0, row] = GameObject.Wall;
            this.board[this.board.GetLength(0) - 1, row] = GameObject.Wall;
        }

        // Generar aleatoriamente objetos
        this.Generate(10, GameObject.Enemy);
        this.Generate(5, GameObject.Life);
        this.Generate(20, GameObject.Trap);
    }

    private void Generate(int count, GameObject obj)
    {
        int generated = 0;

        while (generated < count)
        {
            int col = random.Next(0, this.Width);
            int row = random.Next(0, this.Height);

            if (this.ObjectAt(col, row) == GameObject.Floor)
            {
                this.board[col, row] = obj;
                generated += 1;
            }
        }
    }

    public GameObject ObjectAt(int col, int row)
    {
        return this.board[col, row];
    }

    public int Width { get { return this.board.GetLength(0); } }

    public int Height { get { return this.board.GetLength(1); } }

    public int PlayerRow { get; private set; }

    public int PlayerCol { get; private set; }

    public int Lives { get; private set; }

    public void MovePlayer(Direction direction)
    {
        int newCol = PlayerCol;
        int newRow = PlayerRow;

        // Vamos a calcular la nueva posición en la quedaría el jugador
        switch (direction)
        {
            case Direction.Up:
                newRow -= 1;
                break;
            case Direction.Down:
                newRow += 1;
                break;
            case Direction.Left:
                newCol -= 1;
                break;
            case Direction.Right:
                newCol += 1;
                break;
        }

        // El jugador puede moverse si y solo si esa posición no es una pared
        if (this.ObjectAt(newCol, newRow) != GameObject.Wall)
        {
            this.PlayerCol = newCol;
            this.PlayerRow = newRow;
        }
    }

    public void Update()
    {
        // Calculamos según lo que hay en la posición del jugador, que le sucede
        switch (this.ObjectAt(this.PlayerCol, this.PlayerRow))
        {
            case GameObject.Life:
                this.Lives += 1;
                this.board[this.PlayerCol, this.PlayerRow] = GameObject.Floor;
                break;
            case GameObject.Trap:
                this.Lives -= 1;
                this.board[this.PlayerCol, this.PlayerRow] = GameObject.Floor;
                break;
            case GameObject.Enemy:
                this.Lives -= 1;
                // Hacer saltar al jugador
                MovePlayer((Direction)random.Next(4));
                break;
        }

        // Ahora movemos a cada enemigo
        // Primero vamos a ver donde están, y luego actualizamos cada uno
        for (int col = 0; col < this.Width; col++)
        {
            for (int row = 0; row < this.Height; row++)
            {
                if (this.ObjectAt(col, row) == GameObject.Enemy)
                {
                    UpdateEnemy(col, row);
                }
            }
        }

        this.Lives = Math.Max(0, this.Lives);
    }

    public int CountEnemies()
    {
        int enemies = 0;

        for (int col = 0; col < this.Width; col++)
        {
            for (int row = 0; row < this.Height; row++)
            {
                if (this.ObjectAt(col, row) == GameObject.Enemy)
                {
                    enemies++;
                }
            }
        }

        return enemies;
    }

    private void UpdateEnemy(int col, int row)
    {
        int newCol = col;
        int newRow = row;

        // Aleatoriamente decidimos hacia donde se mueve
        // Con probabilidad 0.5 se mueve hacia el jugador, o hacia una posición aleatoria
        if (this.random.NextDouble() < 0.5)
        {
            // Se mueve hacia el jugador

            if (this.PlayerCol < newCol)
            {
                newCol -= 1;
            }
            else if (this.PlayerCol > newCol)
            {
                newCol += 1;
            }

            if (this.PlayerRow < newRow)
            {
                newRow -= 1;
            }
            else if (this.PlayerRow > newRow)
            {
                newRow += 1;
            }
        }
        else
        {
            // Se mueve aleatorio
            newCol += random.Next(3) - 1;
            newRow += random.Next(3) - 1;
        }

        if (ValidPos(newCol, newRow) && this.ObjectAt(newCol, newRow) == GameObject.Floor)
        {
            this.board[col, row] = GameObject.Floor;
            this.board[newCol, newRow] = GameObject.Enemy;
        }
    }

    public void Attack()
    {
        // Vamos a analizar las nueve posiciones alrededor del jugador
        for (int dCol = -1; dCol <= 1; dCol++)
        {
            for (int dRow = -1; dRow <= 1; dRow++)
            {
                int col = this.PlayerCol + dCol;
                int row = this.PlayerRow + dRow;

                // Si hay un enemigo, se va!
                if (ValidPos(col, row) && this.ObjectAt(col, row) == GameObject.Enemy)
                {
                    this.board[col, row] = GameObject.Corpse;
                    // Pero solo se elimina uno en cada turno
                    return;
                }
            }
        }
    }

    public bool ValidPos(int col, int row)
    {
        return col >= 0 && col < this.Width && row >= 0 && row <= this.Height;
    }
}

// EJERCICIOS

// 1. Modifique el juego para incluir modificadores de ataque que aumentan el rango y la cantidad de enemigos que se matan en cada ataque.
//    Los modificadores de ataque pueden aparecer aleatoriamente cuando se mata un enemigo.

// 2. Modifique el movimiento de las arañas para que la probabilidad de moverse hacia el jugador sea mayor mientras más cerca estén del mismo.

// 3. Modifique las vidas para que aparezcan y desaparezcan cada cierta cantidad de turnos, y que nunca haya más de una en el mapa.
