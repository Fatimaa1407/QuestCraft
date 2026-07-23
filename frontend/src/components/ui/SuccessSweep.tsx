import { motion } from 'framer-motion';

export function SuccessSweep() {
  return (
    <motion.div
      className="pointer-events-none fixed inset-x-0 top-0 z-[60] h-1.5 origin-left bg-gradient-to-r from-emerald-400 via-emerald-500 to-teal-400 shadow-lg shadow-emerald-500/50"
      initial={{ scaleX: 0, opacity: 1 }}
      animate={{ scaleX: 1, opacity: [1, 1, 0] }}
      transition={{ duration: 1.1, times: [0, 0.7, 1], ease: 'easeOut' }}
    />
  );
}
