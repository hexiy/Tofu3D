using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace TofuEngine.Physics;

public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
{
    /// <summary>
    /// Performs any required initialization logic after the Simulation instance has been constructed.
    /// </summary>
    /// <param name="simulation">Simulation that owns these callbacks.</param>
    public void Initialize(Simulation simulation)
    {
        //In this demo, we don't need to initialize anything.
        //If you had a simulation with per body gravity stored in a CollidableProperty<T> or something similar, having the simulation provided in a callback can be helpful.
    }

    /// <summary>
    /// Gets how the pose integrator should handle angular velocity integration.
    /// </summary>
    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

    /// <summary>
    /// Gets whether the integrator should use substepping for unconstrained bodies when using a substepping solver.
    /// If true, unconstrained bodies will be integrated with the same number of substeps as the constrained bodies in the solver.
    /// If false, unconstrained bodies use a single step of length equal to the dt provided to Simulation.Timestep. 
    /// </summary>
    public readonly bool AllowSubstepsForUnconstrainedBodies => false;

    /// <summary>
    /// Gets whether the velocity integration callback should be called for kinematic bodies.
    /// If true, IntegrateVelocity will be called for bundles including kinematic bodies.
    /// If false, kinematic bodies will just continue using whatever velocity they have set.
    /// Most use cases should set this to false.
    /// </summary>
    public readonly bool IntegrateVelocityForKinematics => false;

    public Vector3 Gravity;

    public PoseIntegratorCallbacks(Vector3 gravity) : this()
    {
        Gravity = gravity;
    }

    //Note that velocity integration uses "wide" types. These are array-of-struct-of-arrays types that use SIMD accelerated types underneath.
    //Rather than handling a single body at a time, the callback handles up to Vector<float>.Count bodies simultaneously.
    Vector3Wide gravityWideDt;

    /// <summary>
    /// Callback invoked ahead of dispatches that may call into <see cref="IntegrateVelocity"/>.
    /// It may be called more than once with different values over a frame. For example, when performing bounding box prediction, velocity is integrated with a full frame time step duration.
    /// During substepped solves, integration is split into substepCount steps, each with fullFrameDuration / substepCount duration.
    /// The final integration pass for unconstrained bodies may be either fullFrameDuration or fullFrameDuration / substepCount, depending on the value of AllowSubstepsForUnconstrainedBodies. 
    /// </summary>
    /// <param name="dt">Current integration time step duration.</param>
    /// <remarks>This is typically used for precomputing anything expensive that will be used across velocity integration.</remarks>
    public void PrepareForIntegration(float dt)
    {
        //No reason to recalculate gravity * dt for every body; just cache it ahead of time.
        gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
    }

    /// <summary>
    /// Callback for a bundle of bodies being integrated.
    /// </summary>
    /// <param name="bodyIndices">Indices of the bodies being integrated in this bundle.</param>
    /// <param name="position">Current body positions.</param>
    /// <param name="orientation">Current body orientations.</param>
    /// <param name="localInertia">Body's current local inertia.</param>
    /// <param name="integrationMask">Mask indicating which lanes are active in the bundle. Active lanes will contain 0xFFFFFFFF, inactive lanes will contain 0.</param>
    /// <param name="workerIndex">Index of the worker thread processing this bundle.</param>
    /// <param name="dt">Durations to integrate the velocity over. Can vary over lanes.</param>
    /// <param name="velocity">Velocity of bodies in the bundle. Any changes to lanes which are not active by the integrationMask will be discarded.</param>
    public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
        BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt,
        ref BodyVelocityWide velocity)
    {
        //This also is a handy spot to implement things like position dependent gravity or per-body damping.
        //We don't have to check for kinematics; IntegrateVelocityForKinematics returns false in this type, so we'll never see them in this callback.
        //Note that these are SIMD operations and "Wide" types. There are Vector<float>.Count lanes of execution being evaluated simultaneously.
        //The types are laid out in array-of-structures-of-arrays (AOSOA) format. That's because this function is frequently called from vectorized contexts within the solver.
        //Transforming to "array of structures" (AOS) format for the callback and then back to AOSOA would involve a lot of overhead, so instead the callback works on the AOSOA representation directly.
        velocity.Linear += gravityWideDt;
    }
}