using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;
using PieChart;


namespace PieChart.ViitorCloud
{
    public class CovidManager : MonoBehaviour
    {

        public GameObject pieChartGO;
        public PieChart pieChart;

        public GameObject totalPerStateCellPrefab;
        public GameObject totalPerStateScrollView;
        Transform totalPerStateScrollViewTransform;

        public GameObject totalPerDistrictScrollView;
        Transform totalPerDistrictScrollViewTransform;
        JSONNode allData;

        bool firstClick = true;
        GameObject currentPieChart;


        void Start()
        {
            totalPerStateScrollViewTransform = totalPerStateScrollView.GetComponent<Transform>();
            totalPerDistrictScrollViewTransform = totalPerDistrictScrollView.GetComponent<Transform>();
            StartCoroutine(GetData());
        }

        IEnumerator GetData()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.covid19india.org/v3/data-2020-07-11.json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                allData = JSON.Parse(www.downloadHandler.text);
                int totalConfirmed = allData["MH"]["total"]["confirmed"];
                // Debug.Log(totalConfirmed);
                // Debug.Log(allData.Count);
                // if (node.Tag == JSONNodeType.Object)

                foreach (KeyValuePair<string, JSONNode> data in (JSONObject)allData)
                {
                    GameObject instance = Instantiate(totalPerStateCellPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    instance.transform.SetParent(totalPerStateScrollViewTransform);

                    Text StateCode = instance.transform.GetChild(0).gameObject.GetComponent<Text>();
                    StateCode.text = data.Key;

                    Text StateTotal = instance.transform.GetChild(1).gameObject.GetComponent<Text>();
                    StateTotal.text = data.Value["total"]["confirmed"].Value.ToString();

                    instance.GetComponent<Button>().onClick.AddListener(() => stateButtonClicked(data.Key));

                    // Debug.Log(data.Key);
                    // Debug.Log(data.Value["total"]["confirmed"].Value);
                }

                // Or retrieve results as binary data
                // byte[] results = www.downloadHandler.data;
            }
        }

        public void stateButtonClicked(string key)
        {
            clearDistrictScrollView();
            foreach (KeyValuePair<string, JSONNode> data in allData[key]["districts"])
            {
                GameObject instance = Instantiate(totalPerStateCellPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                instance.transform.SetParent(totalPerDistrictScrollViewTransform);

                Text StateCode = instance.transform.GetChild(0).gameObject.GetComponent<Text>();
                StateCode.text = data.Key;

                Text StateTotal = instance.transform.GetChild(1).gameObject.GetComponent<Text>();
                StateTotal.text = data.Value["total"]["confirmed"].Value.ToString();

                instance.GetComponent<Button>().onClick.AddListener(() => districtButtonClicked(key, data.Key));
            }
        }

        private void districtButtonClicked(string stateKey, string districtKey)
        {
            if (!firstClick)
            {
                Destroy(currentPieChart);
            }
            currentPieChart = Instantiate(pieChartGO, new Vector3(0, 0, 0), Quaternion.identity);
            pieChart = currentPieChart.GetComponent<PieChart>();

            // Debug.Log(stateKey);
            // Debug.Log(districtKey);
            float confirmed = allData[stateKey]["districts"][districtKey]["total"]["confirmed"];
            float recovered = allData[stateKey]["districts"][districtKey]["total"]["recovered"];

            // float tested = allData[stateKey][districtKey]["total"]["tested"];
            pieChart.segments = 2;
            pieChart.Data = new float[] { confirmed, recovered };
            pieChart.dataDescription.Clear();
            pieChart.dataDescription.Add("Confirmed");
            pieChart.dataDescription.Add("Recovered");
            // pieChart.dataDescription.Add("Tested");
            pieChart.customColors = new Color[] { Color.red, Color.blue };
            if (!firstClick)
            {
                pieChart.GetComponent<PieChartMeshController>().GenerateChart(2, PieChartMeshController.AnimationType.Rotation, false);
                // pieChart.ReGenerate();
            }
            currentPieChart.SetActive(true);
            firstClick = false;
        }

        private void clearDistrictScrollView()
        {
            foreach (Transform district in totalPerDistrictScrollViewTransform)
            {
                Destroy(district.gameObject);
            }
        }

    }

}
