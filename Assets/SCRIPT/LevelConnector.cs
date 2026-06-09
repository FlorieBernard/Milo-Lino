using System.Collections.Generic;
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
    [SerializeField] private ParticleSystem[] _commonVfx = System.Array.Empty<ParticleSystem>();
    [SerializeField] private ParticleSystem[] _miloVfx   = System.Array.Empty<ParticleSystem>();
    [SerializeField] private ParticleSystem[] _linoVfx   = System.Array.Empty<ParticleSystem>();

    /// <summary>The name of the scene to load after this one.</summary>
    public string NextScene => _nextScene;

    /// <summary>Grey intensity when Lino is active. 0 = no change, 1 = full grey.</summary>
    public float LinoDarkness => _linoDarkness;

    /// <summary>VFX toujours actifs, quel que soit le personnage.</summary>
    public IReadOnlyList<ParticleSystem> CommonVfx => _commonVfx;

    /// <summary>VFX actifs uniquement en mode Milo (ex. feuilles).</summary>
    public IReadOnlyList<ParticleSystem> MiloVfx => _miloVfx;

    /// <summary>VFX actifs uniquement en mode Lino (ex. pluie).</summary>
    public IReadOnlyList<ParticleSystem> LinoVfx => _linoVfx;
}
