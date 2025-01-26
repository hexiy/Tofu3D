using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;

//// RIGIDBODY
using Rigidbody = Scripts.Rigidbody;
//// 

//// SHAPES
using BoxShape = Scripts.BoxShape;
using SphereShape = Scripts.SphereShape;

////

namespace Tofu3D.Physics;

public class PhysicsController
{
    public bool Running = true;

    // private Dictionary<int, Rigidbody> _rigidbodies = new Dictionary<int, Rigidbody>();
    private List<Rigidbody> _rigidbodies = new List<Rigidbody>();
    private Task _physicsTask;
    private Simulation _simulation;
    private BufferPool _bufferPool;
    private ThreadDispatcher _threadDispatcher;

    public void Init()
    {
        //The buffer pool is a source of raw memory blobs for the engine to use.
        _bufferPool = new BufferPool();
        //The following sets up a simulation with the callbacks defined above, and tells it to use 8 velocity iterations per substep and only one substep per solve.
        //It uses the default SubsteppingTimestepper. You could use a custom ITimestepper implementation to customize when stages run relative to each other, or to insert more callbacks.         
        _simulation = Simulation.Create(_bufferPool, new NarrowPhaseCallbacks(),
            new PoseIntegratorCallbacks(new Vector3(0, -9f, 0)), new SolveDescription(8, 1));

        //Any IThreadDispatcher implementation can be used for multithreading. Here, we use the BepuUtilities.ThreadDispatcher implementation.
        _threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);

        Scene.SceneDisposed += OnSceneDisposed;
        //Now take 100 time steps!
        for (int i = 0; i < 100; ++i)
        {
            //Multithreading is pretty pointless for a simulation of one ball, but passing a IThreadDispatcher instance is all you have to do to enable multithreading.
            //If you don't want to use multithreading, don't pass a IThreadDispatcher.

            //Note that each timestep is 0.01 units in duration, so all 100 time steps will last 1 unit of time.
            //(Usually, units of time are defined to be seconds, but the engine has no preconceived notions about units. All it sees are the numbers.)
        }

        //If you intend to reuse the BufferPool, disposing the simulation is a good idea- it returns all the buffers to the pool for reuse.
        //Here, we dispose it, but it's not really required; we immediately thereafter clear the BufferPool of all held memory.
        //Note that failing to dispose buffer pools can result in memory leaks.

        _physicsTask = Task.Run(PhysicsLoop);
    }

    private void OnSceneDisposed()
    {
        _simulation.Clear();
        _rigidbodies.Clear();
    }


    // private void JitterWorldOnPreStep(float dt)
    // {
    //     UpdatePhysicsWorldRigidbodyData();
    // }
    //
    // private void JitterWorldOnPostStep(float dt)
    // {
    //     UpdateSceneRigidbodyData();
    // }

    private void PhysicsLoop()
    {
        try
        {
            while (true)
            {
                if (Running && Global.GameRunning)
                {
                    UpdatePhysicsWorldRigidbodyData();

                    var a = Stopwatch.StartNew();
                    _simulation.Timestep(Time.FixedDeltaTime, _threadDispatcher);
                    a.Stop();
                    // Wait(Time.FixedDeltaTime -
                    // a.Elapsed.TotalSeconds); // if update took 5 ms, and deltaTime is 15 ms, only wait for 10 ms

                    UpdateSceneRigidbodyData();
                    Wait(Time.FixedDeltaTime);
                }
                else
                {
                    Wait(0.3f); // wait if physics is disabled
                }
            }

            _simulation.Dispose();
            _threadDispatcher.Dispose();
            _bufferPool.Clear();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception:{ex.Message}");
            throw ex;
        }
    }


    private void UpdatePhysicsWorldRigidbodyData()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            if (rigidbody.BodyHandle != null)
            {
                _simulation.Bodies[rigidbody.BodyHandle.Value].MotionState.Pose.Position = rigidbody.Transform.WorldPosition;
            }

            if (rigidbody.StaticHandle != null)
            {
                _simulation.Statics[rigidbody.StaticHandle.Value].Pose.Position = rigidbody.Transform.WorldPosition;
            }
        }
//         
//         foreach (JitterRigidbody jitterWorldRigidBody in JitterWorld.RigidBodies)
//         {
//             if (jitterWorldRigidBody.Tag == null || _rigidbodies.ContainsKey((int)jitterWorldRigidBody.Tag) == false)
//             {
//                 continue;
//             }
//
//             Rigidbody sceneRigidbody = _rigidbodies[(int)jitterWorldRigidBody.Tag];
//             jitterWorldRigidBody.Position = sceneRigidbody.Transform.WorldPosition;
//             // jitterWorldRigidBody.Orientation = Quaternion.FromEulerAnglesInDegrees(sceneRigidbody.Transform.WorldRotation);
//             jitterWorldRigidBody.AngularVelocity = new Vector3(10, 0, 0);
//
//
//             // Vector3 eulers = new Vector3(45, 90, 120);
//             // Quaternion quaternion = Quaternion.FromEulerAnglesInDegrees(eulers);
//             // Vector3 eulersBack = Quaternion.ToEulerAngles(quaternion);
//
//
//             /*if (sceneRigidbody.FirstPhysicsEnabledShape.ShapeType is ShapeType.Box)
//             {
//                 BoxShape sceneRigidbodyBoxShape =
//                     (sceneRigidbody.FirstPhysicsEnabledShape as BoxShape);
//
//                 Jitter2.Collision.Shapes.BoxShape jitterBoxShape =
//                     jitterWorldRigidBody.Shapes[0] as JitterBoxShape;
//
//                 if (jitterBoxShape.Size != sceneRigidbodyBoxShape.Size)
//                 {
//                     jitterBoxShape.Size = sceneRigidbodyBoxShape.Size;
//                 }
//             }
//
//             if (sceneRigidbody.FirstPhysicsEnabledShape.ShapeType is ShapeType.Sphere)
//             {
//                 SphereShape? sceneRigidbodySphereShape =
//                     (sceneRigidbody.FirstPhysicsEnabledShape as SphereShape);
//
//                 Jitter2.Collision.Shapes.SphereShape? jitterSphereShape =
//                     jitterWorldRigidBody.Shapes[0] as JitterSphereShape;
//
//                 if (jitterSphereShape != null && sceneRigidbodySphereShape != null)
//                 {
//                     if (jitterSphereShape.Radius != sceneRigidbodySphereShape.Radius)
//                     {
//                         jitterSphereShape.Radius = sceneRigidbodySphereShape!.Radius;
//                     }
//                 }
//             }
//
//             bool isStatic =
//                 sceneRigidbody.GameObject.IsStatic || sceneRigidbody.IsStaticBody;
//             if (jitterWorldRigidBody.IsStatic != isStatic)
//             {
//                 jitterWorldRigidBody.IsStatic = isStatic;
//             }*/
//         }
    }

    private void UpdateSceneRigidbodyData()
    {
        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            if (rigidbody.BodyHandle != null)
            {
                rigidbody.Transform.WorldPosition =
                    _simulation.Bodies[rigidbody.BodyHandle.Value].Dynamics.Motion.Pose.Position;
                // Debug.Log(_simulation.Bodies[rigidbody.BodyHandle.Value].Dynamics.Motion.Pose.Position);
            }

            else if (rigidbody.StaticHandle != null)
            {
                rigidbody.Transform.WorldPosition = _simulation.Statics[rigidbody.StaticHandle.Value].Pose.Position;
            }
        }

        // foreach (JitterRigidbody jitterWorldRigidBody in JitterWorld.RigidBodies)
        // {
        //     if (jitterWorldRigidBody.Tag == null || _rigidbodies.ContainsKey((int)jitterWorldRigidBody.Tag) == false)
        //     {
        //         continue;
        //     }
        //
        //     Rigidbody sceneRigidbody = _rigidbodies[(int)jitterWorldRigidBody.Tag];
        //     sceneRigidbody.Transform.WorldPosition = jitterWorldRigidBody.Position;
        //     sceneRigidbody.Transform.Rotation = Quaternion.ToEulerAngles(jitterWorldRigidBody.Orientation);
        // }
    }

    private void Step()
    {
        // JitterWorld.Step(Time.FixedDeltaTime, true);
        // Debug.Log($"box position:{_boxRb.Position}");
        // lock (World)
        // {
        // World.Step(Time.fixedDeltaTime);
        // }
    }

    public void AddRigidbody(Rigidbody rb)
    {
        if (_rigidbodies.Contains(rb))
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

        bool isStatic = rb.GameObject.IsStatic || rb.IsStaticBody;
        if (shape.ShapeType is ShapeType.Box)
        {
            Vector3 boxShapeSize = (shape as BoxShape).Size;

            Box box = new Box(boxShapeSize.X, boxShapeSize.Y, boxShapeSize.Z);
            var inertia = box.ComputeInertia(1);

            if (isStatic == false)
            {
                rb.BodyHandle = _simulation.Bodies.Add(
                    BodyDescription.CreateDynamic(
                        new RigidPose(rb.Transform.WorldPosition),
                        inertia, _simulation.Shapes.Add(box), 0.01f));
                
                _simulation.Bodies[rb.BodyHandle.Value].MotionState.Pose.Position = rb.Transform.WorldPosition;

            }
            else
            {
                rb.StaticHandle = _simulation.Statics.Add(new StaticDescription(
                    new RigidPose(rb.Transform.WorldPosition),
                    _simulation.Shapes.Add(box), ContinuousDetection.Continuous()));
                
                _simulation.Statics[rb.StaticHandle.Value].Static.Pose.Position = rb.Transform.WorldPosition;

            }
        }

        _rigidbodies.Add(rb);
        // JitterRigidbody jitterRigidbody = JitterWorld.CreateRigidBody();
        //
        // jitterRigidbody.Position = rb.Transform.WorldPosition;
        // jitterRigidbody.Orientation = Quaternion.FromEulerAnglesInDegrees(rb.Transform.WorldRotation);
        // jitterRigidbody.IsStatic = rb.GameObject.IsStatic || rb.IsStaticBody;
        // jitterRigidbody.Tag = rb.GameObjectId;
        //
        // if (shape.ShapeType is ShapeType.Box)
        // {
        //     Vector3 boxShapeSize = (shape as BoxShape).Size;
        //     var jitterBoxShape = new Jitter2.Collision.Shapes.BoxShape(boxShapeSize.X, boxShapeSize.Y, boxShapeSize.Z);
        //     jitterRigidbody.AddShape(jitterBoxShape);
        // }
        //
        // if (shape.ShapeType is ShapeType.Sphere)
        // {
        //     float sphereShapeRadius = (shape as SphereShape).Radius;
        //
        //     var jitterSphereShape = new Jitter2.Collision.Shapes.SphereShape(sphereShapeRadius);
        //     jitterRigidbody.AddShape(jitterSphereShape);
        // }
        //
        // _rigidbodies.Add(rb.GameObjectId, rb);
        // Debug.Log($"Added rigidbody id {rb.GameObjectId}");
    }

    public void RemoveRigidbody(Rigidbody sceneRigidbody)
    {
        // if (_rigidbodies.Contains(sceneRigidbody.GameObjectId) == false)
        // {
        //     return;
        // }
        //
        // JitterRigidbody? jitterRigidbody =
        //     JitterWorld.RigidBodies.FirstOrDefault((rb) => (int)(rb.Tag ?? -1) == sceneRigidbody.GameObjectId, null);
        // if (jitterRigidbody == null)
        // {
        //     Debug.Log("Cannot remove rigidbody from physics world because body was not found.");
        //     return;
        // }
        //
        // JitterWorld.Remove(jitterRigidbody);
        // _rigidbodies.Remove(sceneRigidbody.GameObjectId);
        //
        // Debug.Log($"Removed rigidbody id {(int)jitterRigidbody.Tag}");
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

    // public void StartPhysics()
    // {
    //     Running = true;
    // }
    //
    // public void StopPhysics()
    // {
    //     Running = false;
    // }
}