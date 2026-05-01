/** Lights, fog, and environment for the 3D scene. Stub — will be fleshed out in a later task. */
export default function SceneSetup() {
  return (
    <group>
      <ambientLight intensity={0.3} />
      <pointLight position={[10, 10, 10]} intensity={1} color="#00e5ff" />
      <fog attach="fog" args={['#0a0a1a', 30, 100]} />
    </group>
  );
}