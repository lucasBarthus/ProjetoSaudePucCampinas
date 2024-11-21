using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour
{

    public Transform bodySegmentPrefab; // Prefab do segmento do corpo
   
    public int initialBodySize = 5; // Número inicial de segmentos do corpo
    public float speed = 2f; // Velocidade da cabeça da cobra
    public float circleRadius = 1f; // Raio do movimento circular
    public float bodySpacing = 0.5f; // Distância entre os segmentos do corpo
    public Vector3 initialPosition = new Vector3(0, 0, 0); // Posição de destino inicial

    private List<Transform> BodySegments = new List<Transform>(); // Segmentos da cobra (incluindo a cabeça)
    private float angle = 0f; // Ângulo para calcular o movimento circular
    private bool isMovingToInitialPosition = true; // Define se a cobra está se movendo até a posição inicial
    private List<Vector3> previousPositions = new List<Vector3>(); // Lista para armazenar as posições anteriores

    void Start()
    {
        // Define a posição inicial da cabeça
        transform.position = initialPosition;

        // Adiciona a cabeça à lista de segmentos
        BodySegments.Add(transform);

        // Instancia os segmentos do corpo
        for (int i = 0; i < initialBodySize; i++)
        {
            Transform segment = Instantiate(bodySegmentPrefab);
            segment.position = transform.position - new Vector3(0, bodySpacing * (i + 1), 0); // Coloca cada segmento em fila
            BodySegments.Add(segment);
            previousPositions.Add(segment.position); // Armazena a posição inicial de cada segmento
        }

        // Armazena a posição inicial da cabeça para o movimento
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
        // Move a cabeça até a posição inicial com velocidade
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, speed * Time.deltaTime);

        // Verifica se a cabeça alcançou a posição inicial
        if (Vector3.Distance(transform.position, initialPosition) < 0.01f)
        {
            isMovingToInitialPosition = false; // Termina o movimento inicial
            previousPositions[0] = transform.position; // Define a posição inicial da cabeça
        }
    }

    void MoveHeadInCircle()
    {
        // Calcula a nova posição da cabeça com movimento circular
        angle += speed * Time.deltaTime;
        float x = initialPosition.x + Mathf.Cos(angle) * circleRadius;
        float y = initialPosition.y + Mathf.Sin(angle) * circleRadius;

        Vector3 newPosition = new Vector3(x, y, 0f);

        // Armazena a posição anterior da cabeça
        previousPositions[0] = transform.position;

        // Calcula a direção do movimento e ajusta a posição da cabeça
        Vector3 direction = newPosition - transform.position;
        transform.position = newPosition;

        // Rotaciona a cabeça suavemente para a direção do movimento no eixo Z
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
            // Verifica se o segmento ainda existe antes de acessar sua posição
            if (BodySegments[i] != null)
            {
                previousPositions[i] = BodySegments[i].position;
                BodySegments[i].position = Vector3.Lerp(BodySegments[i].position, previousPositions[i - 1], Time.deltaTime * speed / bodySpacing);
            }
            else
            {
                // Remove o segmento destruído da lista e também da lista de posições anteriores
                BodySegments.RemoveAt(i);
                previousPositions.RemoveAt(i);
                i--; // Ajusta o índice para compensar a remoção
            }
        }
    }
}