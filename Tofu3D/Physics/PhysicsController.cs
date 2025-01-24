using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Dynamics;
using Jitter2.LinearMath;

namespace Tofu3D.Physics;

public class PhysicsController
{
    public bool Running = true;
    private Dictionary<int, Rigidbody> _rigidbodies = new Dictionary<int, Rigidbody>();
    public World JitterWorld;
    private Task _physicsTask;
    private Stopwatch _sw = new();
    private Jitter2.Dynamics.RigidBody _boxRb;

    public void Init()
    {
        // PhysicsWorld = new PhysicsWorld();

        var capacity = new World.Capacity
        {
            BodyCount = 64_000,
            ContactCount = 128_000,
            ConstraintCount = 32_000,
            SmallConstraintCount = 32_000
        };
        JitterWorld = new World(capacity);


        JitterWorld.DynamicTree.Filter = World.DefaultDynamicTreeFilter;
        JitterWorld.BroadPhaseFilter = null;
        JitterWorld.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
        JitterWorld.Gravity = new JVector(0, -9.81f, 0);
        JitterWorld.SubstepCount = 1;
        JitterWorld.SolverIterations = (8, 4);
        _physicsTask = Task.Run(PhysicsLoop);
    }

    public void PhysicsLoop()
    {
        while (true)
        {
            if (Running /*&& Global.GameRunning*/)
            {
                UpdatePhysicsWorldRigidbodyData();
                // first set physics world position to our gameobjects positions so translations like transformhandle go to physics

                var a = Stopwatch.StartNew();
                Step();

                a.Stop();
                Wait(Time.FixedDeltaTime -
                     a.Elapsed.Seconds); // if update took 5 ms, and deltaTime is 15 ms, only wait for 10 ms

                // aftre physics calcs set our position
                UpdateSceneRigidbodyData();
            }
            else
            {
                Wait(0.3f); // wait if physics is disabled
            }
        }
    }

    private void UpdateSceneRigidbodyData()
    {
        foreach (RigidBody jitterWorldRigidBody in JitterWorld.RigidBodies)
        {
            if (jitterWorldRigidBody.Tag == null || _rigidbodies.ContainsKey((int)jitterWorldRigidBody.Tag) == false)
            {
                continue;
            }

            Rigidbody sceneRigidbody = _rigidbodies[(int)jitterWorldRigidBody.Tag];
            if (sceneRigidbody.IsActive)
            {
                sceneRigidbody.Transform.WorldPosition = jitterWorldRigidBody.Position;
            }
        }
    }

    private void UpdatePhysicsWorldRigidbodyData()
    {
        foreach (RigidBody jitterWorldRigidBody in JitterWorld.RigidBodies)
        {
            if (jitterWorldRigidBody.Tag == null || _rigidbodies.ContainsKey((int)jitterWorldRigidBody.Tag) == false)
            {
                continue;
            }

            Rigidbody sceneRigidbody = _rigidbodies[(int)jitterWorldRigidBody.Tag];
            jitterWorldRigidBody.Position = sceneRigidbody.Transform.WorldPosition;
            if (jitterWorldRigidBody.IsStatic != sceneRigidbody.GameObject.IsStatic)
            {
                jitterWorldRigidBody.IsStatic = sceneRigidbody.GameObject.IsStatic;
            }
        }
    }

    private void Step()
    {
        JitterWorld.Step(Time.FixedDeltaTime, true);

        // Debug.Log($"box position:{_boxRb.Position}");
        // lock (World)
        // {
        // World.Step(Time.fixedDeltaTime);
        // }
    }

    public void AddRigidbody(Rigidbody rb)
    {
        _boxRb = JitterWorld.CreateRigidBody();
        if (rb.Shape.ShapeType != ShapeType.Box)
        {
            Debug.Log($"Shape type {rb.Shape.ShapeType.ToString()} not supported");
            return;
        }

        Vector3 boxShapeSize = (rb.Shape as BoxShape).Size;
        var jitterBoxShape = new Jitter2.Collision.Shapes.BoxShape(boxShapeSize.X, boxShapeSize.Y, boxShapeSize.Z);
        _boxRb.Position = rb.Transform.WorldPosition;
        _boxRb.IsStatic = rb.GameObject.IsStatic;
        _boxRb.Tag = rb.GameObjectId;
        _boxRb.AddShape(jitterBoxShape);

        _rigidbodies.Add(rb.GameObjectId, rb);
    }

    private void Wait(double seconds)
    {
        if (seconds < 0)
        {
            return;
        }

        Thread.Sleep((int)(seconds * 1000f));
        //sw.Restart();
        //
        //while (sw.ElapsedMilliseconds < milliseconds)
        //{
        //
        //}
    }

    public void StartPhysics()
    {
        Running = true;
    }

    public void StopPhysics()
    {
        Running = false;
    }
}