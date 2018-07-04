using UnityEngine;
using RText;
public class RegexExample01 : MonoBehaviour
{
    [SerializeField]
    ClickAbleText _text;

    const string RegexURL = "http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?";
    const string RegexHashtag = "[#＃][Ａ-Ｚａ-ｚA-Za-z一-鿆0-9０-９ぁ-ヶｦ-ﾟー]+";

    void Start()
    {
        _text.SetClickableByRegex(RegexURL, Color.cyan, url => Debug.Log(url));
        _text.SetClickableByRegex(RegexHashtag, Color.green, hashtag => Debug.Log(hashtag));
    }
}
