using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{

    float fall = 0;
    private float fallSpeed;

    //Konstanter som bestämmer om figurerna får roteras / eller måste begränsas
    public bool allowRotation = true;
    public bool limitRotation = false;

    
    private float continousVerticalSpeed = 0.05f; //Farten som figuren rör sig när man håller ner "Ner" knappen.
    private float continousHorizontalSpeed = 0.1f; //Farten som figuren rör sig när man håller i "Höger" eller "Vänster" knappen
    private float buttonDownWaitMax = 0.2f; //Hur länge man ska vänta innan spelet förstår att en knapp är nedtryckt.
    //Kollar om man släpper knappen snabbt
    private bool movedImmediateHorizontal = false;
    private bool movedImmediateVertical = false;

    //Timers för knapptryck
    private float verticalTimer = 0;
    private float horizontalTimer = 0;
    private float buttonDownWaitTimerHorizontal = 0;
    private float buttonDownWaitTimerVertical = 0;

    // Use this for initialization
    void Start()
    {
    //Får fallspeed från fallspeed i Game scriptet som uppdateras när level uppdateras
    fallSpeed = GameObject.Find("GameScript").GetComponent<Game>().fallSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
        CheckUserInput();
    }

    void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            horizontalTimer = 0;
            buttonDownWaitTimerHorizontal = 0;
            movedImmediateHorizontal = false;
           
        }


        if (Input.GetKeyUp(KeyCode.DownArrow))
        {

            movedImmediateVertical = false;
            verticalTimer = 0;
            buttonDownWaitTimerVertical = 0;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }

        if (Input.GetKey(KeyCode.DownArrow) || Time.time - fall >= fallSpeed)
        {
            MoveDown();
        }

        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate();
        }


        }
    /*
     * Röra sig vänster, gör att man kan hålla intryckt eller släppa snabbt och ändå få effekten av att röra sig 1 frame/sek
     */
    void MoveLeft()
    {
        
            if (movedImmediateHorizontal)
            {
                if (buttonDownWaitTimerHorizontal < buttonDownWaitMax)
                {
                    buttonDownWaitTimerHorizontal += Time.deltaTime;
                    return;
                }
                if (horizontalTimer < continousHorizontalSpeed)
                {
                    horizontalTimer += Time.deltaTime;
                    return;
                }
            }
            if (!movedImmediateHorizontal)
                movedImmediateHorizontal = true;
            horizontalTimer = 0;
            transform.position += new Vector3(-1, 0, 0);

            if (CheckIsValidPosition())
            {
                FindObjectOfType<Game>().UpdateGrid(this);
            }
            else
            {
                transform.position += new Vector3(1, 0, 0);
            }
        }

    /*
     * Röra sig Höger, gör att man kan hålla intryckt eller släppa snabbt och ändå få effekten av att röra sig 1 frame/sek
     */
    void MoveRight()
    {
        
            if (movedImmediateHorizontal)
            {
                if (buttonDownWaitTimerHorizontal < buttonDownWaitMax)
                {
                    buttonDownWaitTimerHorizontal += Time.deltaTime;
                    return;
                }

                if (horizontalTimer < continousHorizontalSpeed)
                {
                    horizontalTimer += Time.deltaTime;
                    return;
                }
            }
            if (!movedImmediateHorizontal)
                movedImmediateHorizontal = true;
            horizontalTimer = 0;
            transform.position += new Vector3(1, 0, 0);

            if (CheckIsValidPosition())
            {
                FindObjectOfType<Game>().UpdateGrid(this);
            }
            else
            {
                transform.position += new Vector3(-1, 0, 0);
            }
    }

    /*
     * Rörelse Nedåt, gör att man kan hålla intryckt eller släppa snabbt och ändå få effekten av att röra sig 1 frame/sek
     får även figuren att fortsätta falla nedåt och uppdaterar spelplanen med ny figur om man inte kan röra sig mer. 
     */
    void MoveDown()

    {
        
        if (movedImmediateVertical)
        {
            if (buttonDownWaitTimerVertical < buttonDownWaitMax)
            {
                buttonDownWaitTimerVertical += Time.deltaTime;
                return;
            }
            if (verticalTimer < continousVerticalSpeed)
            {
                verticalTimer += Time.deltaTime;
                return;
            }
        }
        if (!movedImmediateVertical)
            movedImmediateVertical = true;

        verticalTimer = 0;

        transform.position += new Vector3(0, -1, 0);



        if (CheckIsValidPosition())
        {
            FindObjectOfType<Game>().UpdateGrid(this);
        }
        else
        {
            transform.position += new Vector3(0, 1, 0);

            FindObjectOfType<Game>().DeleteRow();

            if (FindObjectOfType<Game>().CheckIsAboveGrid(this))
            {
                FindObjectOfType<Game>().GameOver();
            }
            enabled = false;


            FindObjectOfType<Game>().SpawnNextShape();
        }
        fall = Time.time;
     }
    
    /*
     * Rotation av figurer. Om figurerna ska vara begränsade så kan de bara roteras i 90/-90grader
     */
    void Rotate()
    { 
   
        
            if (allowRotation)
            {
                if (limitRotation)
                {
                    if (transform.rotation.eulerAngles.z >= 90)
                    {
                        transform.Rotate(0, 0, -90);
                    }
                    else
                    {
                        transform.Rotate(0, 0, 90);
                    }
                }
                else
                {
                    transform.Rotate(0, 0, 90);
                }
                if (CheckIsValidPosition())
                {
                    FindObjectOfType<Game>().UpdateGrid(this);
                }
                else
                {
                    if (limitRotation)
                    {
                        if (transform.rotation.eulerAngles.z >= 90)
                        {
                            transform.Rotate(0, 0, -90);
                        }
                        else
                        {
                            transform.Rotate(0, 0, 90);

                        }
                    }
                    else
                    {
                        transform.Rotate(0, 0, -90);
                    }
                }
            }
        }
    
    //Kollar om figuren är inuti spelplanens rutnät och har en korrekt position
    bool CheckIsValidPosition()
    {
        foreach (Transform shape in transform)
        {
            Vector2 pos = FindObjectOfType<Game>().Round(shape.position);

            if (FindObjectOfType<Game>().CheckIsInsideGrid(pos) == false)
            {
                return false;
                
            }
            if (FindObjectOfType<Game>().GetTransformAtGridPosition(pos) != null
                && FindObjectOfType<Game>().GetTransformAtGridPosition(pos).parent != transform)
            {
                return false;
            }
                
        }
        return true;
    }
}
