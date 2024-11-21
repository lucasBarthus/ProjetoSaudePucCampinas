using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour
{

    public Transform bodySegmentPrefab; // Prefab do segmento do corpo
   
    public int initialBodySize = 5; // N�mero inicial de segmentos do corpo
    public float speed = 2f; // Velocidade da cabe�a da cobra
    public float circleRadius = 1f; // Raio do movimento circular
    public float bodySpacing = 0.5f; // Dist�ncia entre os segmentos do corpo
    public Vector3 initialPosition = new Vector3(0, 0, 0); // Posi��o de destino inicial

    private List<Transform> BodySegments = new List<Transform>(); // Segmentos da cobra (incluindo a cabe�a)
    private float angle = 0f; // �ngulo para calcular o movimento circular
    private bool isMovingToInitialPosition = true; // Define se a cobra est� se movendo at� a posi��o inicial
    private List<Vector3> previousPositions = new List<Vector3>(); // Lista para armazenar as posi��es anteriores

    void Start()
    {
        // Define a posi��o inicial da cabe�a
        transform.position = initialPosition;

        // Adiciona a cabe�a � lista de segmentos
        BodySegments.Add(transform);

        // Instancia os segmentos do corpo
        for (int i = 0; i < initialBodySize; i++)
        {
            Transform segment = Instantiate(bodySegmentPrefab);
            segment.position = transform.position - new Vector3(0, bodySpacing * (i + 1), 0); // Coloca cada segmento em fila
            BodySegments.Add(segment);
            previousPositions.Add(segment.position); // Armazena a posi��o inicial de cada segmento
        }

        // Armazena a posi��o inicial da cabe�a para o movimento
        previousPositions.Insert(0, transform.position);
    }

    void Update()
    {
        if (isMovingToInitialPosition)
        {
            MoveToInitialPosition();
        }
        else
        {
            MoveHeadInCircle();
            MoveBodySegments();
        }
    }

    void MoveToInitialPosition()
    {
        // Move a cabe�a at� a posi��o inicial com velocidade
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, speed * Time.deltaTime);

        // Verifica se a cabe�a alcan�ou a posi��o inicial
        if (Vector3.Distance(transform.position, initialPosition) < 0.01f)
        {
            isMovingToInitialPosition = false; // Termina o movimento inicial
            previousPositions[0] = transform.position; // Define a posi��o inicial da cabe�a
        }
    }

    void MoveHeadInCircle()
    {
        // Calcula a nova posi��o da cabe�a com movimento circular
        angle += speed * Time.deltaTime;
        float x = initialPosition.x + Mathf.Cos(angle) * circleRadius;
        float y = initialPosition.y + Mathf.Sin(angle) * circleRadius;

        Vector3 newPosition = new Vector3(x, y, 0f);

        // Armazena a posi��o anterior da cabe�a
        previousPositions[0] = transform.position;

        // Calcula a dire��o do movimento e ajusta a posi��o da cabe�a
        Vector3 direction = newPosition - transform.position;
        transform.position = newPosition;

        // Rotaciona a cabe�a suavemente para a dire��o do movimento no eixo Z
        if (direction != Vector3.zero)
        {
            float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angleZ), Time.deltaTime * speed);
        }
    }
    void MoveBodySegments()
    {
        for (int i = 1; i < BodySegments.Count; i++)
        {
            // Verifica se o segmento ainda existe antes de acessar sua posi��o
            if (BodySegments[i] != null)
            {
                previousPositions[i] = BodySegments[i].position;
                BodySegments[i].position = Vector3.Lerp(BodySegments[i].position, previousPositions[i - 1], Time.deltaTime * speed / bodySpacing);
            }
            else
            {
                // Remove o segmento destru�do da lista e tamb�m da lista de posi��es anteriores
                BodySegments.RemoveAt(i);
                previousPositions.RemoveAt(i);
                i--; // Ajusta o �ndice para compensar a remo��o
            }
        }
    }
}