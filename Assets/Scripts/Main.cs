using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

enum StateCells : ushort
{
    Empty = 0,
    Sheep = 1,
    Wolf = 2,
}

enum LifeDurationAnimals : ushort
{
    Empty = 0,
    Sheep = 0,
    Wolf = 5,
}
struct BufferCell
{
    public StateCells bc_state;
    public Color bc_color;
    public LifeDurationAnimals bc_life;
    
    public BufferCell(StateCells state, Color color)
    {
        bc_color = color;
        bc_state = state;
        bc_life = LifeDurationAnimals.Empty;
    }
}

struct Animal
{
    public int a_x;
    public int a_y;
    public StateCells a_state;
    public LifeDurationAnimals a_life;

    public Animal(int x, int y, StateCells state, LifeDurationAnimals life)
    {
        a_x = x;
        a_y = y;
        a_state = state;
        a_life = life;
    }
}
struct Cell
{
    public StateCells c_state;
    public Material c_material;
    public LifeDurationAnimals c_life;
    public int c_x;
    public int c_y;
    public Cell(Material material, int x, int y)
    {
        c_material = material;
        c_state = StateCells.Empty;
        c_life = LifeDurationAnimals.Empty;
        c_x = x;
        c_y = y;
    }
}
public class Main : MonoBehaviour
{
    private Cell[,] _cells;
    private BufferCell[,] _bufferCells;
    private Dictionary<StateCells, List<int>> _result;
    private List<Animal> _wolfsList;
    private List<Animal> _sheepsList;
    private int _width;
    private int _height;

    [SerializeField]
    private Material defaultColor;
    [SerializeField]
    private Material sheepMaterial;
    [SerializeField]
    private Material wolfMaterial;

    [SerializeField] 
    private GameObject prefab;
    
    void Start()
    {
        _width = 50;
        _height = 50;
        _cells = new Cell[_width, _height];
        _bufferCells = new BufferCell[_width, _height];
        _result = new Dictionary<StateCells, List<int>>()
        {
            { StateCells.Wolf, new List<int>() },
            { StateCells.Sheep, new List<int>() },
            { StateCells.Empty, new List<int>() },
        };
        _wolfsList = new List<Animal>();
        _sheepsList = new List<Animal>();
        
        for (var i = 0; i < _width; i++)
        {
            for (var j = 0; j < _height; j++)
            {
                _cells[i, j] = CreateNewCell(i, j);
                _bufferCells[i, j] = new BufferCell(_cells[i, j].c_state, defaultColor.color);
            }
        }

        InitGameOfLife();
        StartCoroutine(UpdateGame());
    }

    IEnumerator UpdateGame()
    {
        while (true)
        {
            UpdateCells();
            yield return new WaitForSeconds(0.05f); // 1/2 seconds
        }
    }

    private void ClearResultDictionary()
    {
        foreach (var dkv in _result)
        {
            dkv.Value.Clear();
        }
    }
    
    private void ControlPosition(Animal cell)
    {
        var x = cell.a_x;
        var y = cell.a_y;

        for (var i = x - 1; i <= x + 1; i++)
        {
            for (var j = y - 1; j <= y + 1; j++)
            {
                if (i != x || j != y)
                {
                    _result.TryGetValue(_cells[(_width + i) % _width,(_height + j) % _height].c_state,out var tvalue);
                    tvalue.Add((_width + i) % _width);
                    tvalue.Add((_height + j) % _height);
                    _result[_cells[(_width + i) % _width,(_height + j) % _height].c_state] = tvalue;
                }
            }
        }
    }
    private void UpdateWolfPosition()
    {
        List<int> dvSheep;
        BufferCell bufferCell;
        Cell cell;
        int randomPosition;
        var length = _wolfsList.Count;
         
        for (var idx = 0; idx < length; idx++ )
        {
            var _wolf = _wolfsList[idx];
            
            ControlPosition(_wolf);

            _result.TryGetValue(StateCells.Sheep, out dvSheep);

            if (dvSheep.Count > 0)
            {
                _wolf.a_life = LifeDurationAnimals.Wolf;   
            }
            else
            {
                _wolf.a_life -= 1;
                _result.TryGetValue(StateCells.Empty, out dvSheep);
            }

            if (_wolf.a_life > 0)
            {
                randomPosition = Random.Range(0, dvSheep.Count / 2);
                bufferCell = _bufferCells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]];

                if (_sheepsList.Exists(sheep => sheep.a_x == dvSheep[randomPosition * 2] && sheep.a_y == dvSheep[randomPosition * 2 + 1]))
                {
                    var sheep = _sheepsList.First(sheep =>
                        sheep.a_x == dvSheep[randomPosition * 2] && sheep.a_y == dvSheep[randomPosition * 2 + 1]);
                    
                    _sheepsList.Remove(sheep);
                }
                
                bufferCell = _bufferCells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]];
                bufferCell.bc_color = wolfMaterial.color;
                bufferCell.bc_state = _wolf.a_state;
                bufferCell.bc_life = _wolf.a_life;
                _bufferCells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]] = bufferCell;

                bufferCell = _bufferCells[_wolf.a_x, _wolf.a_y];
                bufferCell.bc_color = defaultColor.color;
                bufferCell.bc_state = StateCells.Empty;
                bufferCell.bc_life = LifeDurationAnimals.Empty;
                _bufferCells[_wolf.a_x, _wolf.a_y] = bufferCell;
            
                // updateCell
            
                cell = _cells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]];
                cell.c_material.color = wolfMaterial.color;
                cell.c_state = _wolf.a_state;
                cell.c_life = _wolf.a_life;
                _cells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]] = cell;
            
                cell = _cells[_wolf.a_x, _wolf.a_y];
                cell.c_material.color = defaultColor.color;
                cell.c_state = StateCells.Empty;
                cell.c_life = LifeDurationAnimals.Empty;
                _cells[_wolf.a_x, _wolf.a_y] = cell;

                _wolf.a_x = dvSheep[randomPosition * 2];
                _wolf.a_y = dvSheep[randomPosition * 2 + 1];
            }
            else
            {
                bufferCell = _bufferCells[_wolf.a_x, _wolf.a_y];
                bufferCell.bc_color = defaultColor.color;
                bufferCell.bc_state = StateCells.Empty;
                bufferCell.bc_life = LifeDurationAnimals.Empty;
                _bufferCells[_wolf.a_x, _wolf.a_y] = bufferCell;
                
                cell = _cells[_wolf.a_x, _wolf.a_y];
                cell.c_material.color = defaultColor.color;
                cell.c_state = StateCells.Empty;
                cell.c_life = LifeDurationAnimals.Empty;
                _cells[_wolf.a_x, _wolf.a_y] = cell;
            }

            _wolfsList[idx] =  _wolf;
            dvSheep.Clear();   
            ClearResultDictionary();
        }
    }

    private void UpdateSheepPosition()
    {
        List<int> dvSheep;
        BufferCell bufferCell;
        Cell cell;
        int randomPosition;
        var length = _sheepsList.Count;

        for (var idx = 0; idx < length; idx++)
        {
            var _sheep = _sheepsList[idx];

            ControlPosition(_sheep);

            _result.TryGetValue(StateCells.Empty, out dvSheep);

            randomPosition = Random.Range(0, dvSheep.Count / 2);
            
            // update bufferCell
            bufferCell = _bufferCells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]];
            bufferCell.bc_color = sheepMaterial.color;
            bufferCell.bc_state = _sheep.a_state;
            bufferCell.bc_life = _sheep.a_life;
            _bufferCells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]] = bufferCell;

            bufferCell = _bufferCells[_sheep.a_x, _sheep.a_y];
            bufferCell.bc_color = defaultColor.color;
            bufferCell.bc_state = StateCells.Empty;
            bufferCell.bc_life = LifeDurationAnimals.Empty;
            _bufferCells[_sheep.a_x, _sheep.a_y] = bufferCell;
            
            // updateCell
            
            cell = _cells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]];
            cell.c_material.color = sheepMaterial.color;
            cell.c_state = _sheep.a_state;
            cell.c_life = _sheep.a_life;
            _cells[dvSheep[randomPosition * 2], dvSheep[randomPosition * 2 + 1]] = cell;
            
            cell = _cells[_sheep.a_x, _sheep.a_y];
            cell.c_material.color = defaultColor.color;
            cell.c_state = StateCells.Empty;
            cell.c_life = LifeDurationAnimals.Empty;
            _cells[_sheep.a_x, _sheep.a_y] = cell;
            
            _sheep.a_x = dvSheep[randomPosition * 2];
            _sheep.a_y = dvSheep[randomPosition * 2 + 1];

            _sheepsList[idx] = _sheep;
            
            dvSheep.Clear();
            ClearResultDictionary();
            
        }
    } 
    
    private void InitGameOfLife()
    {
        var randomCellsWolf = Random.Range(50, 150);
        var randomCellsSheep = Random.Range(randomCellsWolf * 10, randomCellsWolf * 20);
        while (randomCellsWolf > 0 && randomCellsSheep > 0)
        {
            for (var i = 0; i < _width; i++)
            {
                for (var j = 0; j < _height; j++)
                {
                    int isNewCell = Random.Range(0, 11); // 1 / 10 to create a new cell

                    if (isNewCell == 0 && _cells[i, j].c_state == StateCells.Empty)
                    {
                        if (randomCellsWolf > 0 && randomCellsSheep <= 0)
                        {
                            GenerateNewAnimal(StateCells.Wolf, wolfMaterial.color,i, j);
                            randomCellsWolf -= 1;
                        } else if (randomCellsSheep > 0 && randomCellsWolf <= 0)
                        {
                            GenerateNewAnimal(StateCells.Sheep, sheepMaterial.color,i, j);
                            randomCellsSheep -= 1;
                        } else if (randomCellsSheep > 0 && randomCellsWolf > 0) {
                            if (Random.Range(0, 2) == 0)
                            {
                                GenerateNewAnimal(StateCells.Sheep, sheepMaterial.color,i, j);
                                randomCellsSheep -= 1;
                            }
                            else
                            {
                                GenerateNewAnimal(StateCells.Wolf, wolfMaterial.color,i, j);
                                randomCellsWolf -= 1;
                            }
                        }
                    }
                }
            }
        }
    }

    private void NewGenerationAnimals()
    {
        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height; y++)
            {
                if (_cells[x,y].c_state != StateCells.Empty )
                {
                    for (var i = x - 1; i <= x + 1; i++)
                    {
                        for (var j = y - 1; j <= y + 1; j++)
                        {
                            if (i != x || j != y)
                            {
                                _result.TryGetValue(_cells[(_width + i) % _width,(_height + j) % _height].c_state,out var tvalue);
                                tvalue.Add((_width + i) % _width);
                                tvalue.Add((_height + j) % _height);
                                _result[_cells[(_width + i) % _width,(_height + j) % _height].c_state] = tvalue;
                            }
                        }
                    }

                    var bufferCell = _bufferCells[x, y];
                    
                    if (_result[StateCells.Wolf].Count == 4)
                    {
                        bufferCell.bc_state = StateCells.Wolf;
                        bufferCell.bc_color = wolfMaterial.color;
                        bufferCell.bc_life = LifeDurationAnimals.Wolf;
                        _wolfsList.Add(new Animal(x, y, StateCells.Wolf, LifeDurationAnimals.Wolf));
                    } else if (_result[StateCells.Sheep].Count == 4 && _result[StateCells.Wolf].Count == 0)
                    {
                        bufferCell.bc_state = StateCells.Sheep;
                        bufferCell.bc_color = sheepMaterial.color;
                        bufferCell.bc_life = LifeDurationAnimals.Sheep;
                        _sheepsList.Add(new Animal(x, y, StateCells.Sheep, LifeDurationAnimals.Sheep));
                    }
                    
                    _bufferCells[x, y] = bufferCell;
                }

            }
        }
    }

    private void GenerateNewAnimal(StateCells state, Color color, int x, int y)
    {
        var cell = _cells[x, y];
        var bufferCell = _bufferCells[x, y];
        var animal = new Animal(x, y, state, LifeDurationAnimals.Empty);
        cell.c_material.color = color;
        cell.c_state = state;
        bufferCell.bc_state = cell.c_state;
        bufferCell.bc_color = cell.c_material.color;
        _cells[x, y] = cell;

        if (cell.c_state == StateCells.Sheep)
        {
            animal.a_life = LifeDurationAnimals.Sheep; 
            _sheepsList.Add(animal);
        }
        else
        {
            animal.a_life = LifeDurationAnimals.Wolf;
            _wolfsList.Add(animal);
        }

        _bufferCells[x, y] = bufferCell;
    }

    private void UpdateCells()
    {
        UpdateWolfPosition();
        _wolfsList = _wolfsList.Where(wolf => wolf.a_life > 0).ToList();
        UpdateSheepPosition();

        NewGenerationAnimals();
        
        for (var i = 0; i < _width; i++)
        {
            for (var j = 0; j < _height; j++)
            {
                var cell = _cells[i, j];
                cell.c_state = _bufferCells[i, j].bc_state;
                cell.c_life = _bufferCells[i, j].bc_life;
                cell.c_material.color = _bufferCells[i, j].bc_color;
                _cells[i, j] = cell;
            }
        }
    }
    private Cell CreateNewCell(int x, int y)
    {
        var go = Instantiate(prefab);
        go.transform.position = new Vector3(x, 0, y);
        go.isStatic = true;
        var boxCollier = go.GetComponent<Collider>();
        boxCollier.enabled = false;
        var renderer = go.GetComponent<Renderer>();
        renderer.receiveShadows = false;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        var material = renderer.material;
        material.color = Color.white;
        return new Cell(material, x , y);
    }
}
