// Sparse twinkling dots to keep large empty stretches of the auth background from feeling dead,
// without competing with the code snippets / cards. Fixed positions (not random) so SSR/CSR match
// and the effect stays subtle and repeatable.
const particles = [
  { top: '12%', left: '82%', size: 3, delay: '0s', color: 'bg-cyan-400' },
  { top: '22%', left: '92%', size: 2, delay: '0.8s', color: 'bg-indigo-400' },
  { top: '35%', left: '88%', size: 2, delay: '1.6s', color: 'bg-cyan-400' },
  { top: '48%', left: '78%', size: 3, delay: '2.4s', color: 'bg-indigo-400' },
  { top: '62%', left: '90%', size: 2, delay: '0.4s', color: 'bg-cyan-400' },
  { top: '74%', left: '84%', size: 2, delay: '1.2s', color: 'bg-indigo-400' },
  { top: '85%', left: '95%', size: 3, delay: '2s', color: 'bg-cyan-400' },
  { top: '18%', left: '68%', size: 2, delay: '2.8s', color: 'bg-indigo-400' },
  { top: '55%', left: '60%', size: 2, delay: '1.8s', color: 'bg-cyan-400' },
  { top: '90%', left: '70%', size: 2, delay: '0.6s', color: 'bg-indigo-400' },
];

export function ParticleField() {
  return (
    <div className="pointer-events-none absolute inset-0 overflow-hidden">
      {particles.map((particle, index) => (
        <span
          key={index}
          className={`animate-twinkle absolute rounded-full ${particle.color}`}
          style={{
            top: particle.top,
            left: particle.left,
            width: particle.size,
            height: particle.size,
            animationDelay: particle.delay,
          }}
        />
      ))}
    </div>
  );
}
