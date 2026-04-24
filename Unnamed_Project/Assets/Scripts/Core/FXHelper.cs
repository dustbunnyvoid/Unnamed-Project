using UnityEngine;

public static class FXHelper
{
    public static void SpawnBurst(Vector3 pos, Color color, int count = 14)
    {
        var go = new GameObject("FX_Burst");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor = color;
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.gravityModifier = 0.5f;
        main.maxParticles = count;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;

        ps.Play();
        Object.Destroy(go, 1f);
    }
}
