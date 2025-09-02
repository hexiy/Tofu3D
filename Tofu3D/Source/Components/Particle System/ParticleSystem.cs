using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scripts;

[ExecuteInEditMode]
public class ParticleSystem : Component, IComponentUpdateable
{
    private readonly Pool<Particle> _pool = new(() => new Particle());
    private ParticleSystemRenderer _renderer;

    private float _time;
    public new bool AllowMultiple = false;

    [Show]
    public Curve ColorCurve = new();

    [Space]
    [Show]
    public ParticleColorType EndColorType;

    public Particle LatestParticle;

    public object ListLock = new();

    [XmlIgnore]
    public ConcurrentQueue<Particle> Particles = new();

    // [Show] public Vector3 StartSize { get; set; } = new(1);
    // [Show] public Vector3 EndSize { get; set; } = new(1);
    [Show]
    public Curve SizeCurve = new();

    [Space]
    [Show]
    public ParticleColorType StartColorType;

    [Show]
    public Vector3 StartVelocity { get; set; } = new(0, 0, 0);

    [Show]
    public float Speed { get; set; } = 2;

    private bool ShowStartColor2 => StartColorType is ParticleColorType.Random;

    [Show]
    public Color StartColor { get; set; } = Color.White;

    [ShowIf(nameof(ShowStartColor2))]
    public Color StartColor2 { get; set; } = Color.Gray;

    private bool ShowEndColor2 => EndColorType is ParticleColorType.Random;

    [Show]
    public Color EndColor { get; set; } = Color.Black;

    [ShowIf(nameof(ShowEndColor2))]
    public Color EndColor2 { get; set; } = Color.Black;

    [Space]
    [Header("Space :3")]
    [Show]
    public int MaxParticles { get; set; } = 1000000;

    [Show]
    public float MaxLifetime { get; set; } = 1;

    [SliderF(0.0001f, 5f)]
    [Show]
    public float SpawnRate { get; set; } = 0.5f; // spawn every half second

    [Show]
    public Vector3 SpawnBoundsSize { get; set; } = new(5, 5, 5); // spawn every half second

    private CancellationTokenSource _particleUpdateCancellation;

    public void Update()
    {
        _time += Time.EditorDeltaTime;
        SpawnRate = Mathf.ClampMin(SpawnRate, 0.0001f);
        while (_time - SpawnRate >= 0 && Particles.Count < MaxParticles)
        {
            SpawnParticle();
            _time -= SpawnRate;
        }

        // Debug.Log(Particles.Count);
        // for (var i = 0; i < Particles.Count; i++)
        //Parallel.For(0, particles.Count, new ParallelOptions() {MaxDegreeOfParallelism = Environment.ProcessorCount * 20}, (i) =>
        //{
    }


    private async Task UpdateParticlesAsync(CancellationToken token)
    {
        const int ChunkSize = 10000;

        while (!token.IsCancellationRequested)
        {
            IEnumerable<Particle[]> particleChunks = Particles
                .ToArray()
                .Chunk(ChunkSize);

            Parallel.ForEach(particleChunks,
                new ParallelOptions() { CancellationToken = token }, chunk =>
                {
                    foreach (Particle particle in chunk) 
                    {
                        particle.Velocity += StartVelocity * Time.EditorDeltaTime;

                        particle.WorldPosition += particle.Velocity * Time.EditorDeltaTime;
                        particle.WorldPosition +=
                            Mathf.Sin(particle.WorldPosition.Y * 0.5f) * 0.2f * new Vector3(1, 0, 1);

                        particle.Lifetime += Time.EditorDeltaTime;

                        // particle.Size = Vector3.Lerp(
                        //     particle.Size,
                        //     Vector3.One * SizeCurve.Sample(particle.Lifetime / MaxLifetime),
                        //     Time.EditorDeltaTime * 50
                        // );
                        particle.Size = Vector3.One * SizeCurve.Sample(particle.Lifetime / MaxLifetime);

                        if (particle.Lifetime > MaxLifetime)
                        {
                            particle.Visible = false;
                        }
                    }
                });

            CleanupParticles();

            await Task.Delay((int)(Time.EditorDeltaTime * 1000));
        }
    }

    private void CleanupParticles()
    {
        while (Particles.TryPeek(out Particle? particle) && particle.Lifetime > MaxLifetime)
        {
            if (Particles.TryDequeue(out Particle? expiredParticle))
            {
                _pool.PutObject(expiredParticle);
            }
        }
    }


    public override void Awake()
    {
        _renderer = GameObject.GetComponent<ParticleSystemRenderer>();
        if (_renderer == null)
        {
            _renderer = GameObject.AddComponent<ParticleSystemRenderer>();
        }

        _renderer.SetParticleSystem(this);


        _particleUpdateCancellation = new CancellationTokenSource();
        _ = UpdateParticlesAsync(_particleUpdateCancellation.Token);

        base.Awake();
    }

    private void SpawnParticle()
    {
        if (Particles.Count >= MaxParticles)
        {
            return;
        }

        Particle p = _pool.GetObject();
        LatestParticle = p;
        p.Visible = true;
        p.Lifetime = 0;
        p.Size = Vector3.One * SizeCurve.Sample(0);
        p.WorldPosition = Transform.WorldPosition;
        p.WorldPosition += new Vector3(Random.Range(-SpawnBoundsSize.X, SpawnBoundsSize.X),
            Random.Range(-SpawnBoundsSize.Y, SpawnBoundsSize.Y), Random.Range(-SpawnBoundsSize.Z, SpawnBoundsSize.Z));

        p.Velocity = StartVelocity;
        p.Color = Random.ColorRange(StartColor, StartColor2);
        p.SpawnColor = p.Color;

        Particles.Enqueue(p);
    }
}