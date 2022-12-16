using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour {
    //Rutnätets bredd och höjd
    public static int gridWidth = 9;
    public static int gridHeight = 20;

    //Rutnätet, var tvungen att hårdkoda bredden p.ga. en bugg som inte löstes på annat sätt...
    public static Transform[,] grid = new Transform[10, gridHeight];

    //Poäng beroende på hur många rader som försvinner, TetrisCounter räknar hur 
    public int scoreOneLine = 100;
    public int scoreTwoLine = 200;
    public int scoreThreeLine = 400;
    public int scoreFourLine = 800;
    public int scoreSuccesive = 1200;
    public int tetrisCounter = 0;

    //Banans hastigt efter specifikationerna
    public float fallSpeed = 0.5f;
    public int currentLevel = 0;

    //Räknar ut hur många rader som förstörs per runda 
    private int numberOfRowsThisTurn = 0;
    private int numberOfRowsDestroyed = 0;
    public Text hud_score;
    public Text hud_level;

    //Score
    private int currentScore = 0;

    private GameObject previewShape;
    private GameObject nextShape;

    private bool gameStarted = false;

    private Vector2 previewShapePosition = new Vector2(-6.5f, 14f);
	
	void Start () {
        SpawnNextShape();

        
	}
    
    void Update()
    {
        UpdateScore();
        UpdateUI();
        UpdateLevel();
        UpdateSpeed();
    }
    //Uppdaterar level var 20e rad som förstörs enligt specen
    void UpdateLevel()
    {
        currentLevel = numberOfRowsDestroyed / 20;
        
        Debug.Log("Current LEvel: " +currentLevel);
        Debug.Log("Current Fallspeed: " + fallSpeed);    }

    //Uppdaterar hastigheten i samband med ny level
    void UpdateSpeed()
    {
        fallSpeed = 0.5f - ((float)currentLevel*0.1f);
        Debug.Log("Current Fallspeed: " + fallSpeed);
    }

    //Uppdaterar UI, buggat denna version
    public void UpdateUI()
    {
        hud_score.text = currentScore.ToString();
        hud_level.text = currentLevel.ToString();
    }

    //Uppdaterar score beroende på hur många rader som förstörs och om man får fler tetris i rad, buggat i denna version
    public void UpdateScore()
    {
        if (numberOfRowsThisTurn > 0)
        {
            if (numberOfRowsThisTurn == 1)
            {
                currentScore += scoreOneLine;
                tetrisCounter =0;
                numberOfRowsDestroyed++;
            }
            else if (numberOfRowsThisTurn == 2)
            {
                currentScore += scoreTwoLine;
                tetrisCounter = 0;
                numberOfRowsDestroyed += 2;
            }
            else if (numberOfRowsThisTurn == 3)
            {
                currentScore += scoreThreeLine;
                tetrisCounter = 0;
                numberOfRowsDestroyed += 3;
            }
            else if (numberOfRowsThisTurn == 4)
            {
                if (tetrisCounter > 0)
                {
                    currentScore += scoreSuccesive;
                    tetrisCounter++;
                    numberOfRowsDestroyed += 4;
                }
                else
                {
                    currentScore += scoreFourLine;
                    tetrisCounter++;
                    numberOfRowsDestroyed += 4;
                }
                
            }
           

            numberOfRowsThisTurn = 0;
        }
    }

    

    //Loopar igenom figurerna och ser om deras position överstiger spelytans gräns i Y-led 
    public bool CheckIsAboveGrid(Shape shapes)
    {
        for(int x = 0; x < 10; ++x)
        {
            foreach(Transform shape in shapes.transform)
            {
                Vector2 pos = Round(shape.position);

                if (pos.y > gridHeight - 1)
                {
                    return true;
                }
            }
        }return false; 
    }

    
    //Itererar över parameter y, kollar varje x position för en transform. Om den hittar en position
    //i raden som returnerar null så vet man att raden inte är full och därför returnerar metoden false.
    //Om den inte returnerar null så är raden full och den returnerar true.
    public bool IsFullRowAt(int y)
    {      
        for (int x=0; x<10; ++x)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
            
        }
        //Om den hittar en full rad ökar den fullrad variabeln. 
        numberOfRowsThisTurn++;
        return true;
    }

    //Förstör raden vid parameter Y
    public void DeleteShapeAt(int y)
    {
        for(int x = 0; x < 10; ++x)
        {
            Destroy(grid[x, y].gameObject);
            grid[x, y] = null;
        }
    }

    //Tar ner ovanstående rader ett steg
    public void MoveRowDown(int y)
    {
        for(int x=0;x<10; ++x)
        {
            if (grid[x, y] != null)
            {
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;
                grid[x, y - 1].position += new Vector3(0, -1, 0);

            }
        }
    }

    //Tar ner alla rader ovanför en viss index(den som tagits bort)
    public void MoveAllRowsDown(int y)
    {
        for(int i=y; i < gridHeight; ++i)
        {
            MoveRowDown(i);
            
        }
    }
    //Samlingsmetod som tar bort rader och flyttar ner ovanstående rader med hjälp av ovanstående metoder
    public void DeleteRow()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsFullRowAt(y))
            {
                DeleteShapeAt(y);

                MoveAllRowsDown(y + 1);

                --y;

                

                             

               
            }

        } 
    }

    //Tarr bort alla gamla positioner i rutnätet och lägger till de nya
    public void UpdateGrid(Shape shape)
    {
        for (int y = 0; y < gridHeight; ++y){
            for(int x =0; x < 10; ++x)
            {
                if (grid[x, y] != null)
                {
                    if (grid[x, y].parent == shape.transform)
                    {
                        grid[x, y] = null;
                    }
                }
            }
        }

        foreach(Transform shapes in shape.transform)
        {
            Vector2 pos = Round(shapes.position);

            if (pos.y < gridHeight)
            {
                grid[(int)pos.x, (int)pos.y] = shapes;
            }
        }
    }

    
    public Transform GetTransformAtGridPosition (Vector2 pos)
    {
        if(pos.y > gridHeight - 1)
        {
            return null;
        }
        else
        {
            return grid[(int)pos.x, (int)pos.y];
        }
    }
      
    /*Skapar första figuren och nästa figur som ska visas genom att ta metoden GetRandomShape som ger en slumpmässig figur
    Skickar sedan nästa figur in i spelet och skapar nästa figur som visas i ruten "Next".
    */
    public void SpawnNextShape()
    {
        if (!gameStarted)
        {
            gameStarted = true;

            nextShape = (GameObject)Instantiate(Resources.Load(GetRandomShape(), typeof(GameObject)), new Vector2(5.0f, 20.0f), Quaternion.identity);
            previewShape = (GameObject)Instantiate(Resources.Load(GetRandomShape(), typeof(GameObject)), new Vector2(-100f,100f), Quaternion.identity);
            previewShape.GetComponent<Shape>().enabled = false;

        }
        else
        {
            
            previewShape.transform.localPosition = new Vector2(5.0f, 20.0f);
            nextShape = previewShape;
            nextShape.GetComponent<Shape>().enabled = true;

            previewShape = (GameObject)Instantiate(Resources.Load(GetRandomShape(), typeof(GameObject)), previewShapePosition, Quaternion.identity);
            previewShape.GetComponent<Shape>().enabled = false;

        }

        
    }
    //Kollar om figurerna är inuti x och y koordinaterna för rutnätet
    public bool CheckIsInsideGrid(Vector2 pos)
    {
        return ((int)pos.x >= 0 && (int)pos.x <= gridWidth && (int)pos.y >= 0);
    }

    //Avrundar positionerna så att det inte ska bli problem med konverteringen från flyttal till Int
    public Vector2 Round(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    //Ger en random figur  som sedan skapas
    string GetRandomShape()
    {
        int randomShape = Random.Range(1, 8);
        string randomShapeName = "Prefabs/T_Shape";

        switch (randomShape)
        {
            case 1:
                randomShapeName = "Prefabs/T_Shape";
                break;
            case 2:
                randomShapeName = "Prefabs/I_Shape";
                break;
            case 3:
                randomShapeName = "Prefabs/L_Shape";
                
                break;
            case 4:
                randomShapeName = "Prefabs/J_Shape";
                
                break;
            case 5:
                randomShapeName = "Prefabs/Z_Shape";
               
                break;
            case 6:
                randomShapeName = "Prefabs/S_Shape";
                break;
            case 7:
                randomShapeName = "Prefabs/O_Shape";
                break;
        }return randomShapeName;

        }
    //Laddar Game over skärmen när spelet tar slut
    public void GameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
    }

