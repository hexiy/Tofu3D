using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jitter2;
using Jitter2.Collision;
using Jitter2.LinearMath;
using Jitter2.SoftBodies;

//// RIGIDBODY
using JitterRigidbody = Jitter2.Dynamics.RigidBody;
using Rigidbody = Scripts.Rigidbody;
//// 

//// SHAPES
using BoxShape = Scripts.BoxShape;
using SphereShape = Scripts.SphereShape;
using JitterBoxShape = Jitter2.Collision.Shapes.BoxShape;
using JitterSphereShape = Jitter2.Collision.Shapes.SphereShape;

////

namespace Tofu3D.Physics;

public class PhysicsController
{
    public bool Running = true;
    private Dictionary<int, Rigidbody> _rigidbodies = new Dictionary<int, Rigidbody>();
    public World JitterWorld;
    private Task _physicsTask;

    public void Init()
    {
        var capacity = new World.Capacity
        {
            BodyCount = 64_000,
            ContactCount = 128_000,
            ConstraintCount = 32_000,
            SmallConstraintCount = 32_000
        };
        JitterWorld = new World(capacity);

        JitterWorld.DynamicTree.Filter = World.DefaultDynamicTreeFilter;
        JitterWorld.BroadPhaseFilter = new BroadPhaseCollisionFilter(JitterWorld);
        JitterWorld.NarrowPhaseFilter = new TriangleEdgeCollisionFilter();
        JitterWorld.Gravity = new JVector(0, -9.81f, 0);
        JitterWorld.SubstepCount = 1;
        JitterWorld.SolverIterations = (8, 4);


        JitterWorld.PreStep += JitterWorldOnPreStep;
        JitterWorld.PostStep += JitterWorldOnPostStep;

        _physicsTask = Task.Run(PhysicsLoop);
    }


    private void JitterWorldOnPreStep(float dt)
    {
        UpdatePhysicsWorldRigidbodyData();
    }

    private void JitterWorldOnPostStep(float dt)
    {
        UpdateSceneRigidbodyData();
    }

    private void PhysicsLoop()
    {
        try
        {
            while (true)
            {
                if (Running && Global.GameRunning)
                {
                    var a = Stopwatch.StartNew();
                    Step();

                    a.Stop();
                    Wait(Time.FixedDeltaTime -
                         a.Elapsed.Seconds); // if update took 5 ms, and deltaTime is 15 ms, only wait for 10 ms
                }
                else
                {
                    Wait(0.3f); // wait if physics is disabled
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception:{ex.Message}");
            throw ex;
        }
    }


    private void UpdatePhysicsWorldRigidbodyData()
    {
        foreach (JitterRigidbody jitterWorldRigidBody in JitterWorld.RigidBodies)
        {
            if (jitterWorldRigidBody.Tag == null || _rigidbodies.ContainsKey((int)jitterWorldRigidBody.Tag) == false)
            {
                continue;
            }

            Rigidbody sceneRigidbody = _rigidbodies[(int)jitterWorldRigidBody.Tag];
            jitterWorldRigidBody.Position = sceneRigidbody.Transform.WorldPosition;
            // jitterWorldRigidBody.Orientation = Quaternion.FromEulerAnglesInDegrees(sceneRigidbody.Transform.WorldRotation);
            jitterWorldRigidBody.AngularVelocity = new Vector3(10, 0, 0);


            // Vector3 eulers = new Vector3(45, 90, 120);
            // Quaternion quaternion = Quaternion.FromEulerAnglesInDegrees(eulers);
            // Vector3 eulersBack = Quaternion.ToEulerAngles(quaternion);


            if (sceneRigidbody.FirstPhysicsEnabledShape.ShapeType is ShapeType.Box)
            {
                BoxShape sceneRigidbodyBoxShape =
                    (sceneRigidbody.FirstPhysicsEnabledShape as BoxShape);

                Jitter2.Collision.Shapes.BoxShape jitterBoxShape =
                    jitterWorldRigidBody.Shapes[0] as JitterBoxShape;

                if (jitterBoxShape.Size != sceneRigidbodyBoxShape.Size)
                {
                    jitterBoxShape.Size = sceneRigidbodyBoxShape.Size;
                }
            }

            if (sceneRigidbody.FirstPhysicsEnabledShape.ShapeType is ShapeType.Sphere)
            {
                SphereShape? sceneRigidbodySphereShape =
                    (sceneRigidbody.FirstPhysicsEnabledShape as SphereShape);

                Jitter2.Collision.Shapes.SphereShape? jitterSphereShape =
                    jitterWorldRigidBody.Shapes[0] as JitterSphereShape;

                if (jitterSphereShape != null && sceneRigidbodySphereShape != null)
                {
                    if (jitterSphereShape.Radius != sceneRigidbodySphereShape.Radius)
                    {
                        jitterSphereShape.Radius = sceneRigidbodySphereShape!.Radius;
                    }
                }
            }

            bool isStatic =
                sceneRigidbody.GameObject.IsStatic || sceneRigidbody.IsStaticBody;
            if (jitterWorldRigidBody.IsStatic != isStatic)
            {
                jitterWorldRigidBody.IsStatic = isStatic;
            }
        }
    }

    private void UpdateSceneRigidbodyData()
    {
        foreach (JitterRigidbody jitterWorldRigidBody in JitterWorld.RigidBodies)
        {
            if (jitterWorldRigidBody.Tag == null || _rigidbodies.ContainsKey((int)jitterWorldRigidBody.Tag) == false)
            {
                continue;
            }

            Rigidbody sceneRigidbody = _rigidbodies[(int)jitterWorldRigidBody.Tag];
            sceneRigidbody.Transform.WorldPosition = jitterWorldRigidBody.Position;
            sceneRigidbody.Transform.Rotation = Quaternion.ToEulerAngles(jitterWorldRigidBody.Orientation);
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
        if (_rigidbodies.ContainsKey(rb.GameObjectId))
        {
            return;
        }

        Scripts.Shape shape = rb.FirstPhysicsEnabledShape;
        if (shape == null)
        {
            return;
        }

        if (shape.ShapeType is not ShapeType.Box && shape.ShapeType is not ShapeType.Sphere)
        {
            Debug.Log($"Shape type {shape.ShapeType.ToString()} not supported");
            return;
        }

        JitterRigidbody jitterRigidbody = JitterWorld.CreateRigidBody();

        jitterRigidbody.Position = rb.Transform.WorldPosition;
        jitterRigidbody.Orientation = Quaternion.FromEulerAnglesInDegrees(rb.Transform.WorldRotation);
        jitterRigidbody.IsStatic = rb.GameObject.IsStatic || rb.IsStaticBody;
        jitterRigidbody.Tag = rb.GameObjectId;

        if (shape.ShapeType is ShapeType.Box)
        {
            Vector3 boxShapeSize = (shape as BoxShape).Size;
            var jitterBoxShape = new Jitter2.Collision.Shapes.BoxShape(boxShapeSize.X, boxShapeSize.Y, boxShapeSize.Z);
            jitterRigidbody.AddShape(jitterBoxShape);
        }

        if (shape.ShapeType is ShapeType.Sphere)
        {
            float sphereShapeRadius = (shape as SphereShape).Radius;

            var jitterSphereShape = new Jitter2.Collision.Shapes.SphereShape(sphereShapeRadius);
            jitterRigidbody.AddShape(jitterSphereShape);
        }

        _rigidbodies.Add(rb.GameObjectId, rb);
        Debug.Log($"Added rigidbody id {rb.GameObjectId}");
    }

    public void RemoveRigidbody(Rigidbody sceneRigidbody)
    {
        if (_rigidbodies.ContainsKey(sceneRigidbody.GameObjectId) == false)
        {
            return;
        }

        JitterRigidbody? jitterRigidbody =
            JitterWorld.RigidBodies.FirstOrDefault((rb) => (int)(rb.Tag ?? -1) == sceneRigidbody.GameObjectId, null);
        if (jitterRigidbody == null)
        {
            Debug.Log("Cannot remove rigidbody from physics world because body was not found.");
            return;
        }

        JitterWorld.Remove(jitterRigidbody);
        _rigidbodies.Remove(sceneRigidbody.GameObjectId);

        Debug.Log($"Removed rigidbody id {(int)jitterRigidbody.Tag}");
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