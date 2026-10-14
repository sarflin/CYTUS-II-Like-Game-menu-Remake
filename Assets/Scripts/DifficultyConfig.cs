using UnityEngine;

[CreateAssetMenu(fileName = "NewDifficulty", menuName = "Cytus2/Difficulty Config")]
public class DifficultyConfig : ScriptableObject
{
    [Header("Difficulty Settings")]
    public string difficultyName;
    public int level;
    public Color difficultyColor;

    [Header("Gameplay")]
    public float noteSpeed = 1f;
    public float healthDrain = 1f;
    public float scoreMultiplier = 1f;

    [Header("Visual")]
    public ParticleSystem hitEffect;
    public ParticleSystem missEffect;
}
