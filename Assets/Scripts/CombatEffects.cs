using UnityEngine;

public static class CombatEffects
{
    public static void CreateBlood(Vector3 position, Vector3 normal)
    {
        CreateBurst(
            "Blood Hit",
            position,
            normal,
            new Color(0.55f, 0.01f, 0.015f),
            20,
            0.035f,
            0.11f,
            1.2f,
            4.5f,
            1.8f
        );
    }

    public static void CreateExplosion(Vector3 position)
    {
        CreateBurst(
            "Missile Explosion",
            position,
            Vector3.up,
            new Color(1f, 0.25f, 0.02f),
            42,
            0.07f,
            0.22f,
            3f,
            9f,
            2.2f
        );

        GameObject lightObject = new GameObject("Explosion Light");
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.22f, 0.02f);
        light.range = 8f;
        light.intensity = 6f;
        Object.Destroy(lightObject, 0.12f);
    }

    static void CreateBurst(
        string objectName,
        Vector3 position,
        Vector3 direction,
        Color color,
        int count,
        float minimumSize,
        float maximumSize,
        float minimumSpeed,
        float maximumSpeed,
        float lifetime)
    {
        GameObject effect = new GameObject(objectName);
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);

        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.45f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minimumSpeed, maximumSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minimumSize, maximumSize);
        main.startColor = color;
        main.gravityModifier = objectName == "Blood Hit" ? 0.8f : 0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = objectName == "Blood Hit" ? 28f : 70f;
        shape.radius = objectName == "Blood Hit" ? 0.03f : 0.2f;

        ParticleSystemRenderer particleRenderer = effect.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (shader != null)
        {
            Material material = new Material(shader);
            material.color = color;
            particleRenderer.material = material;
        }

        particles.Emit(count);
        particles.Play();
        Object.Destroy(effect, lifetime + 0.5f);
    }
}
