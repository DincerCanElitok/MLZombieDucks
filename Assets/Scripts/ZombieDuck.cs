using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ZombieDuck : Agent
{
    [SerializeField] private float speed;
    [SerializeField] private float health;
    [SerializeField] private float damage;
    [SerializeField] private TextMeshProUGUI healthText;
    private Rigidbody2D rb;
    [SerializeField] public Diet diet;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private List<ZombieDuck> ducks;
    [SerializeField] private List<Vegetable> vegetables;
    public Color carnivoreColor, herbivoreColor;
    public bool chooseDiet = false;
    private Animator animator;
    Vector3 startPos;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        HealthUpdate();
        startPos = transform.localPosition;
    }
    public override void OnEpisodeBegin()
    {
        health = 100;
        chooseDiet = false;
        ducks = new List<ZombieDuck>(transform.parent.GetComponent<Habitat>().ducks);
        ducks.Remove(this);
        vegetables = new List<Vegetable>(transform.parent.GetComponent<Habitat>().vegetables);
        transform.localPosition = startPos;
    }
    private void Update()
    {
        if(rb.velocity.normalized.magnitude > 0.1f)
            animator.SetBool("Walk",true);
        else
            animator.SetBool("Walk", false);
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        else
        {
            HealthUpdate();
        }
    }
    private void HealthUpdate()
    {
        healthText.text = health.ToString();
    }
    private void Die()
    {
        AddReward(-10f);
        gameObject.SetActive(false);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        //duck information
        float countOfDucks = 0;
        float closestDuckDistance = Mathf.Infinity;
        Vector2 closestDuckDirection = Vector2.zero;

        
        foreach (ZombieDuck duck in ducks)
        {
            float distance = Vector2.Distance(transform.position, duck.transform.position);
            countOfDucks++;
            if (distance < closestDuckDistance)
            {
                closestDuckDistance = distance;
                closestDuckDirection = (duck.transform.position - transform.position).normalized;
            }
        }

        //if there is only one duck in habitat no need to push information
        if (countOfDucks != 0)
        {
            sensor.AddObservation(countOfDucks);
            sensor.AddObservation(closestDuckDistance);
            sensor.AddObservation(closestDuckDirection.x);
            sensor.AddObservation(closestDuckDirection.y);
        }
        


        //vegetable information
        float countOfVegetables = 0;
        float closestVegetableDistance = Mathf.Infinity;
        Vector2 closestVegetableDirection = Vector2.zero;

        
        foreach (Vegetable vegetable in vegetables)
        {
            float distance = Vector2.Distance(transform.position, vegetable.transform.position);
            countOfVegetables++;

            if (distance < closestVegetableDistance)
            {
                closestVegetableDistance = distance;
                closestVegetableDirection = (vegetable.transform.position - transform.position).normalized;
            }
        }

        sensor.AddObservation(countOfVegetables);
        sensor.AddObservation(closestVegetableDistance);
        sensor.AddObservation(closestVegetableDirection.x);
        sensor.AddObservation(closestVegetableDirection.y);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        
        Vector2 moveDirection = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        rb.AddForce(moveDirection * Time.deltaTime * speed);

        if(!chooseDiet)
        {
            float selectedDietAction = Mathf.Clamp(actions.ContinuousActions[2], 0.0f, 1.0f);
            if (selectedDietAction >= 0.5f)
            {
                diet = Diet.Carnivore;
                spriteRenderer.color = carnivoreColor;
                chooseDiet = true;
            }
            else
            {
                diet = Diet.Herbivore;
                spriteRenderer.color = herbivoreColor;
                chooseDiet = true;
            }
        }
        

    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Vegetable>() != null)
        {
            if(diet == Diet.Herbivore)
            {
                AddReward(5f);             
                collision.gameObject.SetActive(false);
                if (vegetables.Count == 1)
                {
                    AddReward(10f);
                    transform.parent.GetComponent<Habitat>().restartHabitat.Invoke();
                }
                else
                {
                    vegetables.Remove(collision.gameObject.GetComponent<Vegetable>());
                    
                    foreach (var otherDuck in ducks)
                    {
                        otherDuck.vegetables.Remove(collision.gameObject.GetComponent<Vegetable>());
                    }
                }
                    

            }
            else if (diet == Diet.Carnivore)
            {
                AddReward(1f);
                collision.gameObject.SetActive(false);
                if (vegetables.Count == 1)
                {
                    AddReward(5f);
                    transform.parent.GetComponent<Habitat>().restartHabitat.Invoke();
                }
                else
                {
                    vegetables.Remove(collision.gameObject.GetComponent<Vegetable>());

                    foreach (var otherDuck in ducks)
                    {
                        otherDuck.vegetables.Remove(collision.gameObject.GetComponent<Vegetable>());
                    }
                }
            }                                   
        }
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-2f);
            transform.localPosition = startPos;
            //transform.parent.GetComponent<Habitat>().restartHabitat.Invoke();
        }
        else if (collision.gameObject.GetComponent<ZombieDuck>() != null && diet == Diet.Carnivore)
        {
            animator.SetTrigger("Attack");
            collision.gameObject.GetComponent<ZombieDuck>().TakeDamage(damage);
            if(collision.gameObject.activeInHierarchy == false)
            {
                AddReward(10f);
                foreach (var d in ducks)
                {
                    if(d.gameObject.activeInHierarchy == true)
                        return;
                    AddReward(20f);
                    transform.parent.GetComponent<Habitat>().restartHabitat.Invoke();
                }
            }
                
        }
    }
}
public enum Diet { Carnivore, Herbivore }