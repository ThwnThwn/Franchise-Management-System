"use client"

import React, { useRef } from "react"
import { Canvas, useFrame } from "@react-three/fiber"
import { ContactShadows, Float, Environment, OrbitControls, MeshDistortMaterial } from "@react-three/drei"
import * as THREE from "three"
import Link from "next/link"
import { motion } from "framer-motion"
import { Button } from "@heroui/react"

// --- 3D Components ---

function CoffeeCup(props: any) {
    // Procedural Cup: simple geometry combination
    return (
        <group {...props}>
            {/* Cup Body */}
            <mesh position={[0, 0, 0]}>
                <cylinderGeometry args={[0.8, 0.6, 2.4, 32]} />
                <meshStandardMaterial
                    color="#D7CCC8"
                    roughness={0.3}
                    metalness={0.1}
                />
            </mesh>

            {/* Cup Sleeve (Paper Holder) */}
            <mesh position={[0, 0, 0]}>
                <cylinderGeometry args={[0.81, 0.7, 1.2, 32]} />
                <meshStandardMaterial
                    color="#8D6E63"
                    roughness={0.8}
                />
            </mesh>

            {/* Lid (Floating slightly above) */}
            <group position={[0, 1.4, 0]} rotation={[0.1, 0, 0]}>
                <mesh>
                    <cylinderGeometry args={[0.85, 0.85, 0.2, 32]} />
                    <meshStandardMaterial color="#3E2723" roughness={0.2} metalness={0.5} />
                </mesh>
                <mesh position={[0, 0.15, 0]}>
                    <cylinderGeometry args={[0.7, 0.8, 0.15, 32]} />
                    <meshStandardMaterial color="#3E2723" roughness={0.2} metalness={0.5} />
                </mesh>
            </group>

            {/* Decoration: Logo placeholder */}
            <mesh position={[0, 0, 0.81]} rotation={[0, 0, 0]}>
                <circleGeometry args={[0.3, 32]} />
                <meshBasicMaterial color="#3E2723" />
            </mesh>
        </group>
    )
}

function FloatingBeans({ count = 15 }) {
    const beans = useRef<THREE.Group>(null)

    useFrame((state) => {
        if (beans.current) {
            beans.current.rotation.y = state.clock.getElapsedTime() * 0.05
        }
    })

    return (
        <group ref={beans}>
            {Array.from({ length: count }).map((_, i) => (
                <Float
                    key={i}
                    speed={1 + Math.random()}
                    rotationIntensity={2}
                    floatIntensity={2}
                    position={[
                        (Math.random() - 0.5) * 8, // x
                        (Math.random() - 0.5) * 6, // y
                        (Math.random() - 0.5) * 4  // z
                    ]}
                >
                    <mesh rotation={[Math.random() * Math.PI, Math.random() * Math.PI, 0]}>
                        <capsuleGeometry args={[0.08, 0.2, 8, 16]} />
                        <meshStandardMaterial color="#4E342E" roughness={0.6} />
                    </mesh>
                </Float>
            ))}
        </group>
    )
}

function Scene() {
    return (
        <>
            <ambientLight intensity={0.8} />
            <spotLight position={[10, 10, 10]} angle={0.15} penumbra={1} intensity={1} castShadow />
            <pointLight position={[-10, -10, -10]} intensity={0.5} color="#8D6E63" />

            <Environment preset="city" />

            <Float speed={2} rotationIntensity={0.5} floatIntensity={1}>
                {/* Main Hero Cup */}
                <CoffeeCup position={[2, 0, 0]} rotation={[0, -0.5, 0.1]} scale={1.8} />
            </Float>

            {/* Background elements */}
            <FloatingBeans />

            <ContactShadows position={[0, -3, 0]} opacity={0.4} scale={20} blur={2} far={4.5} />
        </>
    )
}

// --- Main Component ---

export default function Hero3D() {
    return (
        <section className="relative w-full h-[90vh] bg-gradient-to-r from-[#5D4037] to-[#3E2723] overflow-hidden">
            {/* 3D Canvas Layer */}
            <div className="absolute inset-0 z-0">
                <Canvas camera={{ position: [0, 0, 8], fov: 45 }}>
                    <Scene />
                    <OrbitControls enableZoom={false} enablePan={false} autoRotate autoRotateSpeed={0.5} />
                </Canvas>
            </div>

            {/* UI Overlay Layer */}
            <div className="relative z-10 w-full h-full max-w-7xl mx-auto px-6 flex flex-col justify-center pointer-events-none">
                <div className="max-w-xl pointer-events-auto">
                    <motion.div
                        initial={{ opacity: 0, x: -50 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ duration: 1, delay: 0.5 }}
                    >
                        <h2 className="text-[#A1887F] font-bold tracking-widest text-sm mb-2 uppercase">
                            Premium Taste
                        </h2>
                        <h1 className="text-7xl md:text-9xl font-black text-white italic leading-[0.85] mb-6 drop-shadow-lg">
                            CÀ <br />
                            HONG BẠN
                        </h1>
                        <p className="text-[#D7CCC8] text-lg mb-8 max-w-md leading-relaxed">
                            Trải nghiệm hương vị cà phê đậm đà, mát lạnh sảng khoái.
                            Được rang xay từ những hạt cà phê thượng hạng nhất.
                        </p>

                        <div className="flex gap-4 items-center">
                            <h3 className="text-4xl font-bold text-[#FFB74D] mr-6">35.000đ</h3>
                            <Link href="/menu">
                                <Button
                                    size="lg"
                                    radius="full"
                                    className="bg-[#FFB74D] text-[#3E2723] font-bold px-8 shadow-xl hover:scale-105 transition-transform"
                                >
                                    Mua ngay
                                </Button>
                            </Link>
                        </div>
                    </motion.div>
                </div>
            </div>

            {/* Decorative Overlay Circles */}
            <div className="absolute top-1/2 left-1/4 w-[500px] h-[500px] bg-[#8D6E63]/20 rounded-full blur-3xl -z-10" />
            <div className="absolute bottom-0 right-0 w-[600px] h-[600px] bg-[#3E2723]/50 rounded-full blur-3xl -z-10" />
        </section>
    )
}
