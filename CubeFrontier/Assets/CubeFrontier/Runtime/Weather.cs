using UnityEngine;

namespace CubeFrontier
{
    /// <summary>Weather system with rain, snow, and storm effects.</summary>
    public sealed class Weather : MonoBehaviour
    {
        public enum WeatherType { Clear, Rain, Snow, Storm, Fog }
        public WeatherType Current { get; private set; } = WeatherType.Clear;
        public float Intensity { get; private set; }

        private float nextChange;
        private float targetIntensity;
        private ParticleSystem rainSystem, snowSystem;
        private Transform target;
        private GameSession session;
        private AudioSource weatherAudio;
        private AudioClip rainClip;

        public void Initialize(GameSession session, Transform player)
        {
            this.session = session;
            this.target = player;
            nextChange = Time.time + Random.Range(60, 180);

            // Create rain particle system
            var rainObj = new GameObject("Rain");
            rainObj.transform.SetParent(transform, false);
            rainSystem = rainObj.AddComponent<ParticleSystem>();
            ConfigureRain(rainSystem);
            rainSystem.Stop();

            // Create snow particle system
            var snowObj = new GameObject("Snow");
            snowObj.transform.SetParent(transform, false);
            snowSystem = snowObj.AddComponent<ParticleSystem>();
            ConfigureSnow(snowSystem);
            snowSystem.Stop();

            // Rain audio
            weatherAudio = gameObject.AddComponent<AudioSource>();
            weatherAudio.playOnAwake = false;
            weatherAudio.loop = true;
            weatherAudio.spatialBlend = 0;
            CreateRainClip();
        }

        private void Update()
        {
            if (!session || session.Paused || !target) return;

            Intensity = Mathf.MoveTowards(Intensity, targetIntensity, Time.deltaTime * .3f);

            if (Time.time > nextChange)
            {
                nextChange = Time.time + Random.Range(90, 300);
                ChangeWeather();
            }

            // Position effects around player
            Vector3 above = target.position + Vector3.up * 20;
            if (rainSystem) rainSystem.transform.position = above;
            if (snowSystem) snowSystem.transform.position = above;

            // Adjust fog based on weather
            if (Current == WeatherType.Storm || Current == WeatherType.Fog)
            {
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, .02f * Intensity, Time.deltaTime);
            }

            weatherAudio.volume = session.Volume * .15f * Intensity;
        }

        private void ChangeWeather()
        {
            float roll = Random.value;
            if (roll < .4f) { SetWeather(WeatherType.Clear); }
            else if (roll < .65f) { SetWeather(WeatherType.Rain); }
            else if (roll < .80f) { SetWeather(WeatherType.Snow); }
            else if (roll < .92f) { SetWeather(WeatherType.Storm); }
            else { SetWeather(WeatherType.Fog); }
        }

        public void SetWeather(WeatherType type)
        {
            Current = type;
            switch (type)
            {
                case WeatherType.Clear:
                    targetIntensity = 0;
                    if (rainSystem) rainSystem.Stop();
                    if (snowSystem) snowSystem.Stop();
                    if (weatherAudio.isPlaying) weatherAudio.Stop();
                    break;
                case WeatherType.Rain:
                    targetIntensity = Random.Range(.4f, .8f);
                    if (rainSystem) { var e = rainSystem.emission; e.rateOverTime = 200 * targetIntensity; rainSystem.Play(); }
                    if (snowSystem) snowSystem.Stop();
                    if (!weatherAudio.isPlaying && rainClip) { weatherAudio.clip = rainClip; weatherAudio.Play(); }
                    break;
                case WeatherType.Snow:
                    targetIntensity = Random.Range(.3f, .7f);
                    if (snowSystem) { var e = snowSystem.emission; e.rateOverTime = 100 * targetIntensity; snowSystem.Play(); }
                    if (rainSystem) rainSystem.Stop();
                    if (weatherAudio.isPlaying) weatherAudio.Stop();
                    break;
                case WeatherType.Storm:
                    targetIntensity = Random.Range(.7f, 1f);
                    if (rainSystem) { var e = rainSystem.emission; e.rateOverTime = 400 * targetIntensity; rainSystem.Play(); }
                    if (snowSystem) snowSystem.Stop();
                    if (!weatherAudio.isPlaying && rainClip) { weatherAudio.clip = rainClip; weatherAudio.Play(); }
                    break;
                case WeatherType.Fog:
                    targetIntensity = Random.Range(.5f, .9f);
                    if (rainSystem) rainSystem.Stop();
                    if (snowSystem) snowSystem.Stop();
                    if (weatherAudio.isPlaying) weatherAudio.Stop();
                    break;
            }
            if (session) session.Notify("Weather: " + type);
        }

        private void ConfigureRain(ParticleSystem ps)
        {
            var main = ps.main;
            main.playOnAwake=false;
            ps.transform.localRotation=Quaternion.Euler(90,0,0);
            main.startLifetime = 1.5f;
            main.startSpeed = 18;
            main.startSize = .04f;
            main.startColor = new Color(.6f, .7f, .85f, .5f);
            main.maxParticles = 800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1.5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30, 30, .1f);

            var emission = ps.emission;
            emission.rateOverTime = 200;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = Resources.Load<Material>("FrontierWeather");
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 4;
        }

        private void ConfigureSnow(ParticleSystem ps)
        {
            var main = ps.main;
            main.playOnAwake=false;
            ps.transform.localRotation=Quaternion.Euler(90,0,0);
            main.startLifetime = 5f;
            main.startSpeed = 2;
            main.startSize = .08f;
            main.startColor = new Color(.95f, .97f, 1f, .7f);
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = .2f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30, 30, .1f);

            var emission = ps.emission;
            emission.rateOverTime = 80;

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1;
            noise.frequency = .5f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = Resources.Load<Material>("FrontierWeather");
        }

        private void CreateRainClip()
        {
            int samples = 44100 * 6;
            float[] data = new float[samples];
            var rng = new System.Random(827);
            float noise = 0;
            for (int i = 0; i < samples; i++)
            {
                float t = i / 44100f;
                noise = noise * .97f + ((float)rng.NextDouble() * 2 - 1) * .03f;
                float patter = Mathf.Sin(t * 800 + Mathf.Sin(t * 3) * 200) * .02f;
                float envelope = Mathf.Sin(Mathf.PI * i / samples);
                data[i] = (noise * .4f + patter) * envelope;
            }
            rainClip = AudioClip.Create("Original rain and storm ambience", samples, 1, 44100, false);
            rainClip.SetData(data, 0);
        }

        private void OnDestroy()
        {
            if (rainClip) Destroy(rainClip);
        }
    }
}
