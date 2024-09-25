using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelperClasses
{
    public static T [] FromJson<T> ( string json )
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>> ( json );
        return wrapper.Items;
    }

    public static string ToJson<T> ( T [] array )
    {
        Wrapper<T> wrapper = new Wrapper<T> ();
        wrapper.Items = array;
        return JsonUtility.ToJson ( wrapper );
    }

    public static string ToJson<T> ( T [] array , bool prettyPrint )
    {
        Wrapper<T> wrapper = new Wrapper<T> ();
        wrapper.Items = array;
        return JsonUtility.ToJson ( wrapper , prettyPrint );
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T [] Items;
    }

    
 
    public static MessageBase FromJsonErrorBase ( string json )
    {
        MessageBase messageBase = JsonUtility.FromJson<MessageBase> ( json );
        return messageBase;
    }

    public static ResponseData FromJsonResponseDataBase ( string json )
    {
        ResponseData responseData = JsonUtility.FromJson<ResponseData> ( json );
        return responseData;
    }
}


[Serializable]
public class MessageBase
{
    public int error;
    public string msg;
}

[Serializable]
public class ResponseData
{
    public string game_high_score;
    public int all_correct_answers;
    //public List<int> collection;
    public string collection;
}

