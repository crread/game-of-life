using System.Collections;
using System.Collections.Generic;
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
struct Cell
{
    public StateCells c_state;
    public Material c_material;
    public LifeDurationAnimals c_life;
    
    public Cell(Material material)
    {
        c_material = material;
        c_state = StateCells.Empty;
        c_life = LifeDurationAnimals.Empty;
    }
}
public class Main : MonoBehaviour
{
    private Cell[,] _cells;
    private BufferCell[,] _bufferCells;
    private List<Cell> _wolfsList;
    private List<Cell> _sheepsList;
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
        _wolfsList = new List<Cell>();
        _sheepsList = new List<Cell>();
        
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
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    UpdateBufferCells(x, y);
                }
            }

            UpdateCells();
            yield return new WaitForSeconds(0.05f); // 1/2 seconds
        }
    }

    private void UpdateWolfPosition()
    {
        
    }

    private void UpdateSheepPostion()
    {
        
    }
    
    //  Update position
    //  - update position wolf
    //      * update life if a sheep is eaten otherwise remove 1hp life
    //  - update position sheep

    //  Generating new generation
    //  - new sheep generation on free space
    //  - new wolf generation on free space
    private void InitGameOfLife()
    {
        var randomCellsWolf = Random.Range(100, 200);
        var randomCellsSheep = Random.Range(randomCellsWolf, randomCellsWolf * 5);

        while (randomCellsWolf > 0 || randomCellsSheep > 0)
        {
            for (var i = 0; i < _width; i++)
            {
                for (var j = 0; j < _height; j++)
                {
                    int isNewCell = Random.Range(0, 11); // 1 / 10 to create a new cell

                    if (isNewCell == 0 && _cells[i, j].c_state == StateCells.Empty)
                    {
                        if (randomCellsWolf > 0 && randomCellsSheep == 0)
                        {
                            GenerateNewAnimal(StateCells.Wolf, wolfMaterial.color,i, j);
                            randomCellsWolf -= 1;
                        } else if (randomCellsSheep > 0 && randomCellsWolf == 0)
                        {
                            GenerateNewAnimal(StateCells.Sheep, sheepMaterial.color,i, j);
                            randomCellsSheep -= 1;
                        } else {
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

    private void GenerateNewAnimal(StateCells state, Color color, int x, int y)
    {
        var cell = _cells[x, y];
        var bufferCell = _bufferCells[x, y]; 
        cell.c_material.color = color;
        cell.c_state = state;
        bufferCell.bc_state = cell.c_state;
        bufferCell.bc_color = cell.c_material.color;
        _cells[x, y] = cell;
        
        if (cell.c_state == StateCells.Sheep)
            _sheepsList.Add(cell);
        else
            _wolfsList.Add(cell);
        
        _bufferCells[x, y] = bufferCell;
    }
    private void UpdateBufferCells(int x, int y)
    {
        var bufferCell = _bufferCells[x, y];
        var emptyCells = 0;

        for (var i = x - 1; i <= x + 1; i++)
        {
            for (var j = y - 1; j <= y + 1; j++)
            {
                if (i != x || j != y)
                {
                    if (_cells[(_width + i) % _width,(_height + j) % _height].c_state == StateCells.Empty)
                        emptyCells += 1;
                }
            }
        }

        if (bufferCell.bc_state == StateCells.Empty)
        {
            if (emptyCells == 5)
            {
                bufferCell.bc_state = StateCells.Wolf;
                bufferCell.bc_color = Color.black;
            }
        }
        else if (emptyCells < 5 || emptyCells > 6)
        {
            bufferCell.bc_state = StateCells.Empty;
            bufferCell.bc_color = Color.white;
        }

        _bufferCells[x, y] = bufferCell;
    }

    private void UpdateCells()
    {
        for (var i = 0; i < _width; i++)
        {
            for (var j = 0; j < _height; j++)
            {
                var cell = _cells[i, j];
                cell.c_state = _bufferCells[i, j].bc_state;
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
        return new Cell(material);
    }
}
