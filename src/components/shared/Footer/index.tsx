"use client"
import React from "react"
import { motion } from "framer-motion"
import {
    Mail,
    Phone,
    MapPin,
    Instagram,
    Facebook,
    Youtube,
    ArrowUpRight,
    Coffee
} from "lucide-react"
import { CHB } from "@/constants/constants"

const Footer = () => {
    const currentYear = new Date().getFullYear()

    const containerVariants = {
        hidden: { opacity: 0 },
        visible: {
            opacity: 1,
            transition: {
                staggerChildren: 0.1
            }
        }
    }

    const itemVariants = {
        hidden: { opacity: 0, y: 20 },
        visible: { opacity: 1, y: 0 }
    }

    return (
        <footer className="bg-[#0f0d0c] w-full text-stone-400 mt-20 pt-24 pb-12 px-8 border-t border-stone-900">
            <motion.div
                max-w-7xl
                mx-auto
                variants={containerVariants}
                initial="hidden"
                whileInView="visible"
                viewport={{ once: true }}
                className="max-w-7xl mx-auto grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-16 mb-20"
            >
                {/* Brand Section */}
                <motion.div variants={itemVariants} className="space-y-8">
                    <div className="flex items-center gap-2 text-white font-serif text-3xl font-bold tracking-tight">
                        <Coffee className="text-amber-600" size={32} strokeWidth={1.5} />
                        <span>{CHB}</span>
                    </div>
                    <p className="leading-relaxed text-sm">
                        Nghệ thuật rang xay thủ công từ những hạt cà phê tuyển chọn nhất vùng cao
                        nguyên Việt Nam. Mang hương vị đậm đà đến từng căn nhà.
                    </p>
                    <div className="flex gap-5">
                        {[Facebook, Instagram, Youtube].map((Icon, idx) => (
                            <a
                                key={idx}
                                href="#"
                                className="hover:text-amber-500 transition-colors duration-300"
                            >
                                <Icon size={20} strokeWidth={1.5} />
                            </a>
                        ))}
                    </div>
                </motion.div>

                {/* Quick Links */}
                <motion.div variants={itemVariants} className="space-y-8">
                    <h4 className="text-white font-semibold uppercase tracking-widest text-sm">
                        Khám phá
                    </h4>
                    <ul className="space-y-4 text-sm">
                        {[
                            "Câu chuyện của chúng tôi",
                            "Thực đơn đặc sắc",
                            "Hợp tác nhượng quyền",
                            "Hệ thống cửa hàng"
                        ].map((link) => (
                            <li key={link}>
                                <a
                                    href="#"
                                    className="flex items-center gap-2 hover:text-white transition-colors group"
                                >
                                    {link}
                                    <ArrowUpRight
                                        size={14}
                                        className="opacity-0 group-hover:opacity-100 transition-all -translate-y-1"
                                    />
                                </a>
                            </li>
                        ))}
                    </ul>
                </motion.div>

                {/* Contact Info */}
                <motion.div variants={itemVariants} className="space-y-8">
                    <h4 className="text-white font-semibold uppercase tracking-widest text-sm">
                        Liên hệ
                    </h4>
                    <ul className="space-y-5 text-sm">
                        <li className="flex gap-4">
                            <MapPin
                                size={18}
                                className="text-amber-600 shrink-0"
                                strokeWidth={1.5}
                            />
                            <span>
                                123 Đường Cà Phê, Quận 1,
                                <br />
                                TP. Hồ Chí Minh
                            </span>
                        </li>
                        <li className="flex items-center gap-4">
                            <Phone
                                size={18}
                                className="text-amber-600 shrink-0"
                                strokeWidth={1.5}
                            />
                            <span>(+84) 0799757249</span>
                        </li>
                        <li className="flex items-center gap-4">
                            <Mail size={18} className="text-amber-600 shrink-0" strokeWidth={1.5} />
                            <span>hellocahongban@chb.com</span>
                        </li>
                    </ul>
                </motion.div>

                {/* Newsletter */}
                <motion.div variants={itemVariants} className="space-y-6">
                    <h4 className="text-white font-semibold uppercase tracking-widest text-sm mb-4">
                        Ghé thăm chúng tôi
                    </h4>
                    <div className="relative group overflow-hidden rounded-xl border border-stone-800 shadow-2xl">
                        {/* Lớp phủ Overlay khi hover */}
                        <div className="absolute inset-0 bg-amber-900/20 opacity-0 group-hover:opacity-100 transition-opacity duration-500 z-10" />

                        <motion.img
                            whileHover={{ scale: 1.05 }}
                            transition={{ duration: 0.6 }}
                            src="/images/footer-location.jpg" // Đường dẫn sau khi đã bỏ vào thư mục public
                            alt="Địa chỉ quán"
                            className="w-full h-48 object-cover object-center"
                        />

                        {/* Phần tag ghi chú nhỏ đè lên ảnh */}
                        <div className="absolute bottom-3 left-3 z-20">
                            <span className="bg-[#0f0d0c]/80 backdrop-blur-md text-white text-[10px] px-3 py-1 rounded-full border border-white/10 tracking-widest uppercase">
                                Open Daily
                            </span>
                        </div>
                    </div>
                    <p className="text-xs text-stone-500 italic leading-relaxed">
                        * Hình ảnh thực tế tại không gian tầng trệt chi nhánh Quận 1.
                    </p>
                </motion.div>
            </motion.div>

            {/* Bottom Bar */}
            <div className="max-w-7xl mx-auto pt-10 border-t border-stone-900 flex flex-col md:flex-row justify-between items-center gap-6">
                <div className="text-[12px] uppercase tracking-widest space-x-6">
                    <a href="#" className="hover:text-white transition-colors">
                        Điều khoản
                    </a>
                    <a href="#" className="hover:text-white transition-colors">
                        Bảo mật
                    </a>
                </div>

                <p className="text-[12px] text-stone-600">
                    © {currentYear} THE CHB. DESIGNED FOR COFFEE LOVERS.
                </p>

                <div className="flex items-center gap-2 text-[12px] font-medium text-stone-500">
                    <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse"></div>
                    Cửa hàng đang mở cửa
                </div>
            </div>
        </footer>
    )
}

export default Footer
