using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class MyAvatarModel
{
    private static readonly ReactiveProperty<List<(string token, string name)>> _avatars =
        new(new List<(string token, string name)>());

    public static IReadOnlyReactiveProperty<List<(string token, string name)>> Avatars => _avatars;

    private static readonly ReactiveProperty<int> _avatarIndex = new(0);
    public static IReadOnlyReactiveProperty<int> AvatarIndex => _avatarIndex;

    public static void SetMyAvatars(IEnumerable<(string token, string name)> avatars)
    {
        var avatarsList = new List<(string token, string name)>
        {
            ("default_female", "デフォルト女性"),
            ("default_male", "デフォルト男性")
        };
        foreach (var avatar in avatars)
        {
            avatarsList.Add(avatar);
        }

        _avatars.Value = avatarsList;
    }

    public static void SetMyAvatarIndex(int index)
    {
        _avatarIndex.Value = index;
    }
}
