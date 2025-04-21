using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact", menuName = "Artifacts/Artifact")]
public class Artifact : ScriptableObject
{
    public string artifactName;
    public string description;
    public Sprite icon; 
    public ArtifactType type;
    public float value;

    public enum ArtifactType
    {
        DamageBoost,
        CooldownReduction,
        HealthIncrease,
        SpeedIncrease
        
    }
}