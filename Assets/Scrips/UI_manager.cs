using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class UI_manager : MonoBehaviour
{
    public GameObject agent;

    public int AgentCounter;

    public int agents_per_press = 5;
    public Vector3 Starting_position;

    public Text QueueCounter;  // public if you want to drag your text object in there manually

    public Text Agent_count;

    public Text FrameCounter;

    public bool Frame_Spawner = false;

    public Text Path_time;

    public int odds = 50;
    
    PathfindingManager Manager;

    Pathfinding Finder;

    int frames = 0;

    float timePassed = 0f;

    float second_timer = 0f;

        void Start() 
    {
        Manager = GetComponent<PathfindingManager>();
        Finder = GetComponent<Pathfinding>();

        // QueueCounter = GetComponent<Text>(); 
    }


    // Update is called once per frame
    void Update()
    {

        frames++;

        // QueueCounter.text = "Agents in the queue: " + Manager.results.Count.ToString();
        
        if (Frame_Spawner)
        {
            if(Random.Range(0, 100) < odds)
            {
            CreateAgent();
            }
        }

        if (Input.GetKeyDown("space"))
        {
            Debug.Log("space key was pressed, time to spawn some agents");
            for(int i = 0; i < agents_per_press; i++ )
            {
                CreateAgent();
            }
            if(Frame_Spawner)
            {
                Frame_Spawner = false;
            }
            else Frame_Spawner = true;
        }

        second_timer += Time.deltaTime;
        timePassed += Time.deltaTime;

        if(second_timer > 10f)
        {
            FrameCounter.text = "Paths calculated in the past 10 seconds: " + Manager.paths_calculated.ToString();
            Manager.paths_calculated = 0;
            second_timer = 0f;
        }

        if(timePassed > 1f)
        {
            
            frames = 0;


            
            if (Finder.time_spend.Count > 0)
            {

                
                Path_time.text = "Average time (ms) to compute new path: " + Finder.time_spend.Average().ToString();
                 
                Finder.time_spend = new List<double>();
            }   
            timePassed = 0f;
        } 


        

        Agent_count.text = "Agents in the scene: " + AgentCounter.ToString();

    }

    void CreateAgent (){
        
        AgentCounter++;

        Instantiate(agent, Starting_position, Quaternion.identity);
    }
}
