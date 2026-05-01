import { motion } from 'framer-motion';

/**
 * Full-screen loading state shown during initial data fetch (US-01).
 *
 * Displays a polished cinematic loading animation with a pulsing ring,
 * animated text, and subtle particle hints to set the "command center" tone
 * before the 3D scene renders.
 */
export default function LoadingScreen() {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-950 overflow-hidden">
      {/* Subtle radial gradient background */}
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,rgba(0,229,255,0.04)_0%,transparent_70%)]" />

      {/* Animated concentric rings */}
      <div className="relative flex items-center justify-center">
        {/* Outer ring */}
        <motion.div
          className="absolute w-32 h-32 rounded-full border border-accent-cyan/20"
          animate={{ scale: [1, 1.15, 1], opacity: [0.3, 0.6, 0.3] }}
          transition={{ duration: 3, repeat: Infinity, ease: 'easeInOut' }}
        />

        {/* Middle ring */}
        <motion.div
          className="absolute w-24 h-24 rounded-full border border-accent-purple/30"
          animate={{ rotate: 360 }}
          transition={{ duration: 8, repeat: Infinity, ease: 'linear' }}
        />

        {/* Inner spinning ring */}
        <motion.div
          className="w-16 h-16 rounded-full border-2 border-accent-cyan border-t-transparent"
          animate={{ rotate: 360 }}
          transition={{ duration: 1.2, repeat: Infinity, ease: 'linear' }}
        />

        {/* Center dot pulse */}
        <motion.div
          className="absolute w-3 h-3 rounded-full bg-accent-cyan"
          animate={{ scale: [0.8, 1.2, 0.8], opacity: [0.6, 1, 0.6] }}
          transition={{ duration: 1.5, repeat: Infinity, ease: 'easeInOut' }}
        />
      </div>

      {/* Text below spinner */}
      <div className="absolute bottom-1/3 text-center">
        <motion.p
          className="text-sm font-medium text-gray-400 tracking-wide"
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3, duration: 0.6 }}
          Initializing Command Center
        </motion.p>
        <motion.div
          className="flex items-center justify-center gap-1 mt-2"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          {[0, 1, 2].map((i) => (
            <motion.span
              key={i}
              className="w-1.5 h-1.5 rounded-full bg-accent-cyan/60"
              animate={{ opacity: [0.3, 1, 0.3] }}
              transition={{
                duration: 1.2,
                repeat: Infinity,
                delay: i * 0.2,
                ease: 'easeInOut',
              }}
            />
          ))}
        </motion.div>
      </div>
    </div>
  );
}