using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Quality : MonoBehaviour
{
    Status status;
    public UniversalRenderPipelineAsset DefaultRPA;
    public UniversalRenderPipelineAsset PartyRPA;

    public GameExtraSettings[] extraSettings;

    [Header("Grass")]
    public GameObject Grass;
    public Material GrassMat;
    public Material GrassWindMat;
    public float defaultShadowDistance;

    private void Awake()
    {
        status = FindObjectOfType<Status>();

        OnSettingsChanged();
    }

    public void OnSettingsChanged()
    {
        GameDataManager.GameData GD = GameDataManager.GetGameData();

        RenderQualityUpdate(GD);
        LODUpdate(GD);

        switch (GD.settingsData.windowMode)
        {
            case 0:
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
                break;
            case 1:
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.Windowed);
                break;
            default:
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.ExclusiveFullScreen);
                break;
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        RenderQualityUpdate(GameDataManager.GetGameData());
        LODUpdate(GameDataManager.GetGameData());
    }

    void RenderQualityUpdate(GameDataManager.GameData GD)
    {
        UniversalRenderPipelineAsset RPA = GetRPA();

        switch (GD.settingsData.shadowDistance)
        {
            case 0:
                foreach (Light light in FindObjectsOfType<Light>())
                {
                    light.shadows = LightShadows.None;
                }
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.renderShadows = false;
                }
                RPA.shadowDistance = 0;
                break;

            case 1:
                foreach (Light light in FindObjectsOfType<Light>())
                {
                    light.shadows = LightShadows.Soft;
                }
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.renderShadows = true;
                }
                RPA.shadowDistance = GetShadowDisdance() * .33f;
                break;

            case 2:
                foreach (Light light in FindObjectsOfType<Light>())
                {
                    light.shadows = LightShadows.Soft;
                }
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.renderShadows = true;
                }
                RPA.shadowDistance = GetShadowDisdance() * 1f;
                break;

            default:
                foreach (Light light in FindObjectsOfType<Light>())
                {
                    light.shadows = LightShadows.Soft;
                }
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.renderShadows = true;
                }
                RPA.shadowDistance = GetShadowDisdance() * 2f;
                break;
        }

        switch (GD.settingsData.antiAliasing)
        {
            case 0:
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.antialiasingQuality = AntialiasingQuality.Low;
                    UACD.antialiasing = AntialiasingMode.None;
                }
                RPA.msaaSampleCount = 0;
                break;

            case 1:
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    UACD.antialiasingQuality = AntialiasingQuality.Low;
                }
                RPA.msaaSampleCount = 2;
                break;

            case 2:
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    UACD.antialiasingQuality = AntialiasingQuality.Medium;
                }
                RPA.msaaSampleCount = 4;
                break;

            case 3:
                foreach (UniversalAdditionalCameraData UACD in FindObjectsOfType<UniversalAdditionalCameraData>())
                {
                    UACD.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    UACD.antialiasingQuality = AntialiasingQuality.High;
                }
                RPA.msaaSampleCount = 8;
                break;
        }

        switch (GD.settingsData.textureRes)
        {
            case 0:
                QualitySettings.masterTextureLimit = 2;
                break;

            case 1:
                QualitySettings.masterTextureLimit = 1;
                break;

            default:
                QualitySettings.masterTextureLimit = 0;
                break;
        }

        GraphicsSettings.renderPipelineAsset = RPA;
    }

    public void LODUpdate(GameDataManager.GameData GD)
    {
        int minimumNatureLOD = -1;
        int updatedNatureLOD;
        bool useWindInMedium = true;
        foreach (GameExtraSettings settings in extraSettings)
        {
            if (status.gameIndex == settings.gameIndex)
            {
                minimumNatureLOD = settings.minimumNatureLOD;
                useWindInMedium = !settings.DontUseWindInMedium;
            }
        }
        if (minimumNatureLOD == -1 | GD.settingsData.natureLOD >= minimumNatureLOD)
        {
            updatedNatureLOD = (int)GD.settingsData.natureLOD;
        }
        else
        {
            updatedNatureLOD = minimumNatureLOD;
        }
        switch (updatedNatureLOD)
        {
            case 0:
                foreach (Terrain terrain in FindObjectsOfType<Terrain>())
                {
                    terrain.drawTreesAndFoliage = false;
                }
                break;
            case 1:
                foreach (Terrain terrain in FindObjectsOfType<Terrain>())
                {
                    terrain.drawTreesAndFoliage = true;
                    terrain.detailObjectDistance = 0;
                    terrain.detailObjectDensity = 0;
                    TreePrototype[] trees = terrain.terrainData.treePrototypes;

                    foreach (TreePrototype t in trees)
                    {
                        if (!t.prefab.GetComponent<Nature>().isTree)
                        {
                            t.prefab.GetComponent<LODGroup>().size = 0;
                        }
                    }
                }
                break;
            case 2:
                foreach (Terrain terrain in FindObjectsOfType<Terrain>())
                {
                    terrain.drawTreesAndFoliage = true;
                    terrain.detailObjectDistance = 0;
                    terrain.detailObjectDensity = 0;
                    TreePrototype[] trees = terrain.terrainData.treePrototypes;

                    foreach (TreePrototype t in trees)
                    {
                        t.prefab.GetComponent<LODGroup>().size = t.prefab.GetComponent<Nature>().defaultScale;
                    }
                }
                break;
            case 3:
                foreach (Terrain terrain in FindObjectsOfType<Terrain>())
                {
                    terrain.drawTreesAndFoliage = true;
                    terrain.detailObjectDistance *= 1;
                    terrain.detailObjectDensity *= 1;

                    print(useWindInMedium);
                    if (useWindInMedium) { Grass.GetComponent<MeshRenderer>().material = GrassWindMat; }
                    else { Grass.GetComponent<MeshRenderer>().material = GrassMat; }

                    TreePrototype[] trees = terrain.terrainData.treePrototypes;

                    foreach (TreePrototype t in trees)
                    {
                        t.prefab.GetComponent<LODGroup>().size = t.prefab.GetComponent<Nature>().defaultScale;
                    }
                }
                break;
            default:
                foreach (Terrain terrain in FindObjectsOfType<Terrain>())
                {
                    terrain.drawTreesAndFoliage = true;
                    terrain.detailObjectDistance *= 3;
                    terrain.detailObjectDensity = 1;
                    Grass.GetComponent<MeshRenderer>().material = GrassWindMat;

                    TreePrototype[] trees = terrain.terrainData.treePrototypes;

                    foreach (TreePrototype t in trees)
                    {
                        try
                        {
                            t.prefab.GetComponent<LODGroup>().size = t.prefab.GetComponent<Nature>().defaultScale * 3;
                        }
                        catch
                        {
                            Debug.LogError("Tree not assaigned in terrain " + terrain.name);
                        }
                    }
                }
                break;
        }


        switch (GD.settingsData.psLOD)
        {
            case 0:
                foreach (ParticleSystem ps in FindObjectsOfType<ParticleSystem>())
                {
                    ps.enableEmission = false;
                }
                break;
            case 1:
                foreach (ParticleSystem ps in FindObjectsOfType<ParticleSystem>())
                {
                    ps.enableEmission = true;
                    ParticleSystem.EmissionModule emission = ps.emission;
                    emission.rateOverTime = emission.rateOverTime.constant * .2f;
                }
                break;
            case 2:
                foreach (ParticleSystem ps in FindObjectsOfType<ParticleSystem>())
                {
                    ps.enableEmission = true;
                    ParticleSystem.EmissionModule emission = ps.emission;
                    emission.rateOverTime = emission.rateOverTime.constant * .5f;
                }
                break;
            default:
                foreach (ParticleSystem ps in FindObjectsOfType<ParticleSystem>())
                {
                    ps.enableEmission = true;
                    ParticleSystem.EmissionModule emission = ps.emission;
                    emission.rateOverTime = emission.rateOverTime.constant;
                }
                break;
        }

        switch (GD.settingsData.standLOD)
        {
            case 0:
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("GrandStand"))
                {
                    go.transform.Find("Spectators").gameObject.SetActive(false);
                }
                break;
            case 1:
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("GrandStand"))
                {
                    for (int i = 0; i < go.transform.Find("Spectators").childCount; i++)
                    {
                        if (Random.Range(0, 3) == 0) { go.transform.Find("Spectators").GetChild(i).gameObject.SetActive(false); }
                        else
                        {
                            go.transform.Find("Spectators").GetChild(i).GetComponent<AnimationRandomSpeed>().enabled = false;
                            go.transform.Find("Spectators").GetChild(i).GetComponent<Animator>().enabled = false;
                        }
                    }
                }
                break;
            case 2:
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("GrandStand"))
                {
                    for (int i = 0; i < go.transform.Find("Spectators").childCount; i++)
                    {
                        if (Random.Range(0, 2) == 0) { go.transform.Find("Spectators").GetChild(i).gameObject.SetActive(false); }
                        else
                        {
                            go.transform.Find("Spectators").GetChild(i).GetComponent<AnimationRandomSpeed>().enabled = true;
                            go.transform.Find("Spectators").GetChild(i).GetComponent<Animator>().enabled = true;
                        }
                    }
                }
                break;
            default:
                foreach (GameObject go in GameObject.FindGameObjectsWithTag("GrandStand"))
                {
                    for (int i = 0; i < go.transform.Find("Spectators").childCount; i++)
                    {
                        go.transform.Find("Spectators").GetChild(i).GetComponent<AnimationRandomSpeed>().enabled = true;
                        go.transform.Find("Spectators").GetChild(i).GetComponent<Animator>().enabled = true;
                    }
                }
                break;
        }
    }

    UniversalRenderPipelineAsset GetRPA()
    {
        UniversalRenderPipelineAsset RPA;

        if (FindObjectOfType<Methods>().IsSceneGame())
        {
            if (FindObjectOfType<Functions>().RPA is null)
            {
                RPA = DefaultRPA;
            }
            else
            {
                RPA = FindObjectOfType<Functions>().RPA;
            }
        }
        else if (status.isSceneParty)
        {
            RPA = PartyRPA;
        }
        else
        {
            RPA = DefaultRPA;
        }
        return RPA;
    }

    float GetShadowDisdance()
    {
        float distance;

        if (FindObjectOfType<Methods>().IsSceneGame())
        {
            if (FindObjectOfType<Functions>().RPA is null)
            {
                distance = defaultShadowDistance;
            }
            else
            {
                distance = FindObjectOfType<Functions>().RPA.shadowDistance;
            }
        }
        else
        {
            distance = defaultShadowDistance;
        }
        return distance;
    }

    public int psLODMaxRandomNumber()
    {
        int lod = (int)GameDataManager.GetGameData().settingsData.psLOD;

        switch (lod)
        {
            case 1:
                return 4;
            case 2:
                return 2;
            default:
                return 0;
        }
    }

    [System.Serializable]
    public class GameExtraSettings
    {
        public int gameIndex;
        public bool DontUseWindInMedium;
        public int minimumNatureLOD;
    }
}
