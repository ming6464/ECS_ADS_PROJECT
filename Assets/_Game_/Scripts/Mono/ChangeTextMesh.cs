using System;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ChangeTextMesh : MonoBehaviour
{
    public TextMeshPro textPrefab;
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
        return;
        bool hasData = false;
        foreach (var textData in _textDataArr)
        {
            if (textData.id == textMeshData.id)
            {
                hasData = true;
                textData.textMesh.text = textMeshData.text.ToString();
                if (disableText)
                {
                    textData.textMesh.gameObject.SetActive(false);
                    _textDataArr.Remove(textData);
                }
                break;
            }
        }

        if (!hasData && !disableText)
        {
            var textNew = Instantiate(textPrefab, textMeshData.position, quaternion.identity);
            textNew.transform.parent = transform;
            textNew.text = textMeshData.text.ToString();
            _textDataArr.Add(new TextData()
            {
                id = textMeshData.id,
                textMesh = textNew
            });
        }
    }
}

[Serializable]
public struct TextData
{
    public int id;
    public TextMeshPro textMesh;
}