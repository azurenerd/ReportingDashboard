export default function SceneSetup() {
  return (
    <>
      <ambientLight intensity={0.3} />
      <directionalLight position={[10, 10, 5]} intensity={0.5} />
      <pointLight position={[-10, -10, -5]} intensity={0.2} color="#00f0ff" />
      <fog attach="fog" args={['#030712', 20, 80]} />
    </>
  );
}