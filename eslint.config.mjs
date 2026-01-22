import js from "@eslint/js"
import tsEslint from "@typescript-eslint/eslint-plugin"
import tsParser from "@typescript-eslint/parser"
import pluginNext from "@next/eslint-plugin-next"
import pluginReact from "eslint-plugin-react"
import pluginReactHooks from "eslint-plugin-react-hooks"
import globals from "globals"

export default [
    js.configs.recommended,
    {
        files: ["**/*.{js,jsx,ts,tsx}"],
        languageOptions: {
            parser: tsParser,
            globals: {
                ...globals.browser,
                ...globals.node
            }
        },
        plugins: {
            react: pluginReact,
            "react-hooks": pluginReactHooks,
            "@next/next": pluginNext,
            "@typescript-eslint": tsEslint
        },
        rules: {
            // React
            ...pluginReact.configs.flat.recommended.rules,
            ...pluginReactHooks.configs.recommended.rules,

            // Next.js
            ...pluginNext.configs.recommended.rules,
            ...pluginNext.configs["core-web-vitals"].rules,

            // Ts
            "no-unused-vars": "off",
            "@typescript-eslint/no-unused-vars": "error",

            // Hooks
            "react-hooks/rules-of-hooks": "error",
            "react-hooks/exhaustive-deps": "warn"
        },
        settings: {
            react: {
                version: "detect"
            }
        }
    }
]
