using System;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ChangeTextMesh : MonoBehaviour
{
    public TextTMP_Setup textPrefab;
    //
    private List<TextData> _textDataArr;
    private bool _isInitEvent;

    private void Start()
    {
        _textDataArr = new List<TextData>();
    }

    public void Update()
    {
        if (!_isInitEvent)
        {
            UpdateHybrid playerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<UpdateHybrid>();
            if(playerSystem == null) return;
            playerSystem.UpdateText += ChangeText;
            _isInitEvent = true;
        }
    }

    private void ChangeText(TextMeshData textMeshData,bool disableText)
    {
        bool hasData = false;
        foreach (var textData in _textDataArr)
        {
            if (textData.id == textMeshData.id)
            {
                hasData = true;
                if (disableText)
                {
                    textData.textTMP.Off();
                    _textDataArr.Remove(textData);
                }
                else
                {
                    textData.textTMP.ChangeText(textMeshData.text.ToString());
                }
                break;
            }
        }

        if (!hasData && !disableText)
        {
            var textNew = Instantiate(textPrefab, textMeshData.position, quaternion.identity);
            textNew.SetUp(textMeshData.offset,textMeshData.textFollowPlayer);
            textNew.ChangeText(textMeshData.text.ToString());
            _textDataArr.Add(new TextData()
            {
                id = textMeshData.id,
                textTMP = textNew
            });
        }
    }
}

[Serializable]
public struct TextData
{
    public int id;
    public TextTMP_Setup textTMP;
}