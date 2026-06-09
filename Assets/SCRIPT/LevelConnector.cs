using UnityEngine;

/// <summary>
/// Place this component in each scene to define which scene comes next
/// and the visual ambiance parameters for Lino mode.
/// </summary>
public class LevelConnector : MonoBehaviour
{
    [SerializeField] private string _nextScene;

    [Header("Lino Mode")]
    [Range(0f, 1f)]
    [SerializeField] private float _linoDarkness = 1f;
    [SerializeField] private ParticleSystem _miloVfx;
    [SerializeField] private ParticleSystem _linoVfx;

    /// <summary>The name of the scene to load after this one.</summary>
    public string NextScene => _nextScene;

    /// <summary>Grey intensity when Lino is active. 0 = no change, 1 = full grey.</summary>
    public float LinoDarkness => _linoDarkness;

    /// <summary>Ambient VFX active while playing as Milo (e.g. leaves). Can be null.</summary>
    public ParticleSystem MiloVfx => _miloVfx;

    /// <summary>Ambient VFX active while playing as Lino (e.g. rain). Can be null.</summary>
    public ParticleSystem LinoVfx => _linoVfx;
}
