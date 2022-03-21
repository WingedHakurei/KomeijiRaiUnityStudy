using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox
{
    public MessageBox(string messageInfo, string firstText, string secondText)
    {
        UnityEngine.Object asset = Resources.Load("Prefabs/MessageBox");

        go = UnityEngine.Object.Instantiate(asset) as GameObject;

        go.transform.Find("Bg/MessageBox/MessageInfo").GetComponent<Text>().text = messageInfo;

        Transform first = go.transform.Find("Bg/MessageBox/First");

        first.Find("Text").GetComponent<Text>().text = firstText;

        first.GetComponent<Button>().onClick.AddListener(() => Result = BoxResult.First);

        Transform second = go.transform.Find("Bg/MessageBox/Second");

        second.Find("Text").GetComponent<Text>().text = secondText;

        second.GetComponent<Button>().onClick.AddListener(() => Result = BoxResult.Second);
    }

    public async Task<BoxResult> GetReplyAsync()
    {
        return await Task.Run<BoxResult>(() =>
        {
            while (true)
            {
                if (Result != BoxResult.None)
                {
                    return Result;
                }
            }
        });
    }

    public void Close()
    {
        GameObject.Destroy(go);
    }

    public BoxResult Result { get; set; }

    private GameObject go;

    public enum BoxResult
    {
        /// <summary>
        /// 还未出结果
        /// </summary>
        None,

        /// <summary>
        /// 选项1
        /// </summary>
        First,

        /// <summary>
        /// 选项2
        /// </summary>
        Second,
    }
}
