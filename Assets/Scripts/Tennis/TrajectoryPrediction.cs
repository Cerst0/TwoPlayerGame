using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrajectoryPrediction : MonoBehaviour //Singleton<TrajectoryPrediction>
{
    public GameObject TennisBall;
    bool stopp;
    bool UIenabled;

    Scene currentScene;
    Scene predictionScene;

    PhysicsScene currentPhysicsScene;
    PhysicsScene predictionPhysicsScene;

    public Image PredictionGround;

    LineRenderer lineRenderer;
    Status status;
    GameObject dummy;

    void Start()
    {
        status = FindObjectOfType<Status>();

        Physics.autoSimulation = false;

        currentScene = SceneManager.GetActiveScene();
        currentPhysicsScene = currentScene.GetPhysicsScene();

        CreateSceneParameters parameters = new(LocalPhysicsMode.Physics3D);
        predictionScene = SceneManager.CreateScene("Prediction", parameters);
        predictionPhysicsScene = predictionScene.GetPhysicsScene();

        lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
        if (currentPhysicsScene.IsValid())
        {
            currentPhysicsScene.Simulate(Time.fixedDeltaTime);
        }
    }

    public void DisableUI()
    {
        lineRenderer.enabled = false;
        if (UIenabled)
        {
            LeanTween.value(PredictionGround.gameObject, PredictionGround.transform.parent.GetComponent<CanvasGroup>().alpha, 0, 0.2f).setOnUpdate((float val) =>
            {
                PredictionGround.transform.parent.GetComponent<CanvasGroup>().alpha = val;
            });
            UIenabled = false;
        }
    }

    void EnableUI()
    {
        lineRenderer.enabled = true;
        if (!UIenabled)
        {
            LeanTween.value(PredictionGround.gameObject, PredictionGround.transform.parent.GetComponent<CanvasGroup>().alpha, .8f, 0.2f).setIgnoreTimeScale(true).setOnUpdate((float val) =>
            {
                PredictionGround.transform.parent.GetComponent<CanvasGroup>().alpha = val;
            });
            UIenabled = true;
        }
    }

    public void Predict(Vector3 currentPosition, Vector3 force, int id, bool calledByServer = false)
    {
        if (NetworkClient.active && !calledByServer) { status.sync.PredictTennisBall(currentPosition, force, id); }
        if (currentPhysicsScene.IsValid() && predictionPhysicsScene.IsValid())
        {
            if (dummy == null)
            {
                dummy = Instantiate(TennisBall);
                Destroy(dummy.GetComponent<TennisBall>());
                Destroy(dummy.GetComponent<NetworkTransform>());
                Destroy(dummy.GetComponent<NetworkIdentity>());
                dummy.GetComponent<Rigidbody>().useGravity = true;
                dummy.GetComponent<Rigidbody>().isKinematic = false;
                dummy.GetComponent<Rigidbody>().velocity = Vector3.zero;
                dummy.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                SceneManager.MoveGameObjectToScene(dummy, predictionScene);
            }

            dummy.transform.position = currentPosition;

            stopp = false;
            dummy.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
            lineRenderer.positionCount = 0;

            LeanTween.value(PredictionGround.gameObject, PredictionGround.color.a, PredictionGround.transform.parent.GetComponent<CanvasGroup>().alpha, 0).setOnUpdate((float val) =>
            {
                PredictionGround.transform.parent.GetComponent<CanvasGroup>().alpha = val;
            });

            for (int i = 0; i < 200; i++)
            {
                if (!stopp)
                {
                    lineRenderer.positionCount++;
                    predictionPhysicsScene.Simulate(Time.fixedDeltaTime);
                    lineRenderer.SetPosition(i, dummy.transform.position);
                    if (dummy.transform.position.y < 0)
                    {
                        PredictionGround.transform.position = new Vector3(lineRenderer.GetPosition(i).x, PredictionGround.transform.position.y, lineRenderer.GetPosition(i).z);
                        PredictionGround.color = FindObjectOfType<Methods>().GetPlayerColor(id);
                        stopp = true;
                    }
                }
            }

            EnableUI();
            Destroy(dummy);
        }
    }
}