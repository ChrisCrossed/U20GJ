using UnityEngine;

public class NewEmptyCSharpScript : MonoBehaviour
{

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject OtherBackground;

    void Start()
    {
        
    }

    private void Update()
    {
        if(player.transform.position.x - transform.position.x > 1050)
        {
            transform.position = OtherBackground.transform.position + new Vector3(450, 0, 0);
        }
    }
}
