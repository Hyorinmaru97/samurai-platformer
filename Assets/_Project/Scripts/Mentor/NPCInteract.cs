using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCInteract : MonoBehaviour
{
    [Header("Диалог")]
    [TextArea(2, 5)]
    public string[] dialogLines;

    [Header("UI")]
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI npcNameText;
    public GameObject interactHint;

    [Header("Настройки")]
    public string npcName = "Ментор";
    public float interactDistance = 2f;
    public float autoCloseDistance = 3.5f;
    public float typeSpeed = 0.03f;

    Transform player;
    bool playerInRange;
    bool isDialogOpen;
    int currentLine;
    bool isTyping;
    bool dialogFinished;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        if (npcNameText) npcNameText.text = npcName;
        if (dialogPanel) dialogPanel.SetActive(false);
        if (interactHint) interactHint.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // Флип в сторону игрока
        float dir = player.position.x - transform.position.x;
        Vector3 scale = transform.localScale;
        scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = dist <= interactDistance;

        // Автозакрытие если игрок далеко ушёл
        if (isDialogOpen && dist > autoCloseDistance)
        {
            CloseDialog();
            return;
        }

        if (interactHint) interactHint.SetActive(playerInRange && !isDialogOpen);

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isDialogOpen)
                OpenDialog();
            else if (!isTyping)
                NextLine();
        }

        if (isDialogOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseDialog();
    }

    void OpenDialog()
    {
        isDialogOpen = true;
        if (dialogPanel) dialogPanel.SetActive(true);

        if (dialogFinished)
        {
            // показываем постоянную реплику магазина
            if (dialogText)
                dialogText.text = "[ Перейти в магазин ]";
        }
        else
        {
            currentLine = 0;
            ShowLine(currentLine);
        }
    }

    void NextLine()
    {
        if (dialogFinished)
        {
            // тут потом будет открытие магазина
            CloseDialog();
            return;
        }

        currentLine++;
        if (currentLine >= dialogLines.Length)
        {
            dialogFinished = true;
            if (dialogText)
                dialogText.text = "[ Перейти в магазин ]";
        }
        else
        {
            ShowLine(currentLine);
        }
    }

    void CloseDialog()
    {
        isDialogOpen = false;
        if (dialogPanel) dialogPanel.SetActive(false);
    }

    void ShowLine(int index)
    {
        if (dialogText)
            StartCoroutine(TypeLine(dialogLines[index]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (char c in line)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }
}
