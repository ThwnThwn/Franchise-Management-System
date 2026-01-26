# TÀI LIỆU CHI TIẾT VỀ LUỒNG FRONTEND - CHB CFMS

## 📋 MỤC LỤC

1. [Tổng Quan Dự Án](#tổng-quan-dự-án)
2. [Kiến Trúc & Công Nghệ](#kiến-trúc--công-nghệ)
3. [Cấu Trúc Thư Mục](#cấu-trúc-thư-mục)
4. [Luồng Routing & Pages](#luồng-routing--pages)
5. [Components & Features](#components--features)
6. [Authentication Flow](#authentication-flow)
7. [State Management](#state-management)
8. [Providers & Context](#providers--context)
9. [Styling & Theming](#styling--theming)
10. [API Integration](#api-integration)

---

## 🎯 TỔNG QUAN DỰ ÁN

**Tên dự án:** Cà Hong Bạn (CHB) - Coffee Management System Frontend  
**Framework:** Next.js 16.1.4 (App Router)  
**React Version:** 19.2.3  
**UI Library:** HeroUI (NextUI fork)  
**State Management:** Zustand, React Query (TanStack Query)  
**Styling:** Tailwind CSS 4, Styled Components  
**Type Safety:** TypeScript 5

**Mục đích:** Hệ thống quản lý quán cà phê với các tính năng đặt món, quản lý đơn hàng, và quản lý người dùng.

---

## 🏗️ KIẾN TRÚC & CÔNG NGHỆ

### Tech Stack

| Công Nghệ | Phiên Bản | Mục Đích |
|-----------|-----------|----------|
| Next.js | 16.1.4 | React framework với App Router |
| React | 19.2.3 | UI library |
| TypeScript | 5 | Type safety |
| HeroUI | 2.8.7 | Component library |
| TanStack Query | 5.90.19 | Server state management |
| Zustand | 5.0.10 | Client state management |
| Axios | 1.13.2 | HTTP client |
| Tailwind CSS | 4 | Utility-first CSS |
| Framer Motion | 12.28.2 | Animation library |
| Formik + Yup | 2.4.9 + 1.7.1 | Form handling & validation |

### Kiến Trúc Tổng Thể

```
┌─────────────────────────────────────────┐
│         Root Layout (layout.tsx)        │
│  - Providers (Query, HeroUI, Toast)    │
│  - Navbar (Global Navigation)          │
│  - Modals (Login, Alert, etc.)         │
└─────────────────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
┌───────▼────────┐    ┌─────────▼────────┐
│  Public Pages  │    │  Protected Pages │
│  - Home        │    │  (Future)        │
│  - Demo        │    │                  │
└────────────────┘    └──────────────────┘
```

---

## 📁 CẤU TRÚC THƯ MỤC

```
chb-cfms-fe/
├── src/
│   ├── app/                    # Next.js App Router
│   │   ├── layout.tsx          # Root layout với providers
│   │   ├── page.tsx            # Trang chủ (/) - Demo UI components
│   │   ├── globals.css         # Global styles
│   │   ├── (public)/           # Public route group
│   │   │   └── home/
│   │   │       └── page.tsx    # Trang home (/home)
│   │   └── demo/
│   │       └── page.tsx       # Trang demo (/demo)
│   │
│   ├── components/             # Shared components
│   │   ├── modals/             # Modal components
│   │   │   ├── AlertModal/     # Alert dialog
│   │   │   └── index.tsx       # Modal registry
│   │   ├── shared/             # Shared UI components
│   │   │   ├── Navbar/         # Navigation bar
│   │   │   │   ├── index.tsx   # Main navbar component
│   │   │   │   └── ProfileDropdown/  # User profile dropdown
│   │   │   └── ClientHydration/      # Client-side hydration wrapper
│   │   └── styled/             # Styled components
│   │       ├── ButtonStyled/   # Custom button
│   │       ├── NavbarStyled/   # Custom navbar
│   │       ├── ModalStyled/   # Custom modal
│   │       └── ...            # Other styled components
│   │
│   ├── features/               # Feature-based modules
│   │   ├── auth/               # Authentication feature
│   │   │   ├── components/
│   │   │   │   └── LoginModal/
│   │   │   │       ├── index.tsx
│   │   │   │       └── LoginForm/
│   │   │   ├── hooks/
│   │   │   │   └── useAuth.ts  # Auth hooks (useLogin)
│   │   │   ├── services/
│   │   │   │   └── authApi.ts  # Auth API calls
│   │   │   └── models/         # Auth data models
│   │   └── home/               # Home feature
│   │       ├── components/
│   │       │   └── Banner/     # Home banner component
│   │       ├── pages/
│   │       │   └── HomeView/   # Home page view
│   │       └── services/       # Home API services
│   │
│   ├── hooks/                  # Custom hooks
│   │   ├── reuseable/         # Reusable hooks
│   │   └── singleton/         # Singleton hooks (stores)
│   │       ├── disclosures/    # Modal disclosure hooks
│   │       │   ├── useLoginDiscloresure.ts
│   │       │   ├── useRegisterDiscloresure.ts
│   │       │   └── useForgotPasswordDiscloresure.ts
│   │       └── store/          # Zustand stores
│   │           ├── useTokenStore.ts    # Token storage
│   │           └── useSideBarItemStore.ts
│   │
│   ├── providers/              # React Context providers
│   │   ├── DisclosureProvider/ # Modal disclosure context
│   │   └── index.tsx           # AppProviders wrapper
│   │
│   ├── constants/              # Constants & configs
│   │   ├── strings.ts          # String constants
│   │   ├── constants.ts        # App constants
│   │   ├── enums.ts            # TypeScript enums
│   │   ├── colorMaps.ts        # Color mappings
│   │   └── queryKeys.ts        # React Query keys
│   │
│   ├── utils/                  # Utility functions
│   │   ├── axios.ts            # Axios instance
│   │   ├── axiosServer.ts      # Server-side axios
│   │   └── helpers/            # Helper functions
│   │       ├── jwt.ts          # JWT utilities
│   │       ├── stringHelper.ts
│   │       └── getQueryClient.ts
│   │
│   └── models/                 # TypeScript models
│       └── common/
│           └── respone.ts       # API response types
│
├── public/                     # Static assets
│   └── images/
│       └── avtFallback.jpg     # Avatar fallback image
│
├── package.json
├── tsconfig.json
├── tailwind.config.js
└── next.config.ts
```

---

## 🗺️ LUỒNG ROUTING & PAGES

### Next.js App Router Structure

Dự án sử dụng **Next.js App Router** với cấu trúc file-based routing:

### 1. **Root Route (`/`)**
**File:** `src/app/page.tsx`

**Mô tả:** Trang chủ chính của ứng dụng, hiện tại đang là trang demo UI components.

**Nội dung:**
- Demo các button styles (primary, secondary, success, warning, danger)
- Demo card component với input field
- Hiển thị các styled components để test UI

**Component Tree:**
```
page.tsx
└── ButtonStyled (multiple variants)
└── Card + CardBody
    └── Input (HeroUI)
```

---

### 2. **Home Route (`/home`)**
**File:** `src/app/(public)/home/page.tsx`

**Mô tả:** Trang home chính thức của ứng dụng.

**Component Tree:**
```
page.tsx
└── HomeView (from features/home)
    └── Banner component (placeholder)
```

**Tính năng:**
- Hiển thị banner
- Navigation bar với login button
- Responsive design

---

### 3. **Demo Route (`/demo`)**
**File:** `src/app/demo/page.tsx`

**Mô tả:** Trang demo để test các components và UI patterns.

**Nội dung:**
- Tương tự trang chủ (`/`)
- Demo các UI components
- Test responsive layout

---

### Route Groups

#### `(public)` Route Group
- **Mục đích:** Nhóm các routes công khai không cần authentication
- **Routes:**
  - `/home` - Trang chủ

#### Future Route Groups (Có thể thêm)
- `(protected)` - Routes cần authentication
- `(admin)` - Routes dành cho admin
- `(customer)` - Routes dành cho khách hàng

---

## 🧩 COMPONENTS & FEATURES

### 1. **Navbar Component**

**Location:** `src/components/shared/Navbar/index.tsx`

**Tính năng:**
- ✅ Responsive navigation bar
- ✅ Auto-hide khi scroll down, show khi scroll up
- ✅ Collapse animation khi scroll
- ✅ Mobile menu drawer với animation
- ✅ Active menu highlighting
- ✅ Login button / Profile dropdown (conditional)
- ✅ Logo với link về trang chủ

**State Management:**
- `scrollState`: "default" | "top" | "middle"
- `isHiddenNavbar`: boolean
- `isOpen`: mobile menu state
- `activeMenuKey`: current active menu

**Menu Items:**
- Home (`/`)

**Conditional Rendering:**
```typescript
{isLoggedIn ? (
  <ProfileDropdown />
) : (
  <ButtonStyled onClick={onOpenLogin}>
    Login
  </ButtonStyled>
)}
```

---

### 2. **Login Modal**

**Location:** `src/features/auth/components/LoginModal/`

**Component Structure:**
```
LoginModal/
├── index.tsx          # Modal wrapper
└── LoginForm/
    └── index.tsx      # Login form với validation
```

**Tính năng:**
- ✅ Modal dialog với HeroUI Modal
- ✅ Form validation với Formik + Yup
- ✅ Remember me checkbox
- ✅ Error handling với toast notifications
- ✅ Success callback để đóng modal

**Flow:**
1. User click "Login" button trong Navbar
2. `useLoginDiscloresureSingleton` mở modal
3. User nhập email/password
4. Submit form → gọi `useLogin` hook
5. Success → lưu token vào Zustand store → đóng modal → show toast
6. Error → show error toast

---

### 3. **Profile Dropdown**

**Location:** `src/components/shared/Navbar/ProfileDropdown/index.tsx`

**Tính năng:**
- ✅ Hiển thị khi user đã login
- ✅ Dropdown menu với user actions
- ✅ Logout functionality
- ✅ Avatar display

---

### 4. **Styled Components**

**Location:** `src/components/styled/`

Các styled components được tạo để customize HeroUI components:

| Component | Mục Đích |
|-----------|----------|
| `ButtonStyled` | Custom button với theme colors |
| `NavbarStyled` | Custom navbar styling |
| `ModalStyled` | Custom modal với responsive sizing |
| `CardStyled` | Custom card component |
| `InputStyled` | Custom input với focus states |
| `AvatarStyled` | Custom avatar với fallback |

---

## 🔐 AUTHENTICATION FLOW

### Luồng Đăng Nhập

```
┌─────────────────────────────────────────────────┐
│  1. User clicks "Login" button in Navbar       │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  2. useLoginDiscloresureSingleton.onOpen()     │
│     → Opens LoginModal                         │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  3. User fills LoginForm                       │
│     - Email                                     │
│     - Password                                  │
│     - Remember Me (optional)                    │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│  4. Form submit → useLogin hook                 │
│     → Calls authApi.login(payload)             │
└─────────────────┬───────────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │                   │
        ▼                   ▼
┌──────────────┐   ┌──────────────────┐
│   SUCCESS    │   │     ERROR       │
│              │   │                 │
│ - Save token │   │ - Show error    │
│   to store   │   │   toast         │
│ - Close modal│   │ - Keep modal    │
│ - Show toast │   │   open          │
└──────────────┘   └──────────────────┘
```

### Token Management

**Store:** `src/hooks/singleton/store/useTokenStore.ts`

**Tính năng:**
- Lưu `accessToken` vào Zustand store
- Support `rememberMe` option (localStorage vs sessionStorage)
- Auto clear token khi logout

**Usage:**
```typescript
const isLoggedIn = useTokenStore((s) => !!s.accessToken)
const setAccessToken = useTokenStore((s) => s.setAccessToken)
const clearToken = useTokenStore((s) => s.clearToken)
```

---

## 📦 STATE MANAGEMENT

### 1. **Zustand Stores**

#### Token Store
**Location:** `src/hooks/singleton/store/useTokenStore.ts`

**State:**
- `accessToken`: string | null
- `setAccessToken(token, rememberMe)`: void
- `clearToken()`: void

**Storage:**
- `rememberMe = true` → localStorage
- `rememberMe = false` → sessionStorage

---

#### Disclosure Stores (Modal Management)
**Location:** `src/hooks/singleton/disclosures/`

**Stores:**
- `useLoginDiscloresure` - Login modal state
- `useRegisterDiscloresure` - Register modal state
- `useForgotPasswordDiscloresure` - Forgot password modal state

**Pattern:**
```typescript
const { isOpen, onOpen, onClose, onOpenChange } = useLoginDiscloresureSingleton()
```

---

### 2. **React Query (TanStack Query)**

**Provider:** `src/providers/index.tsx`

**Tính năng:**
- Server state management
- Automatic caching
- Background refetching
- Optimistic updates

**Usage:**
```typescript
const { data, isLoading, error } = useClientFetch('/api/endpoint')
const { mutate } = useMutation({ mutationFn: apiCall })
```

**Query Client:**
- Singleton pattern với `getQueryClient()`
- Configured với default options

---

## 🔄 PROVIDERS & CONTEXT

### AppProviders

**Location:** `src/providers/index.tsx`

**Provider Hierarchy:**
```
QueryClientProvider
└── HeroUIProvider
    └── DisclosureProvider
        └── ToastProvider
            └── {children}
```

**Providers:**
1. **QueryClientProvider** - React Query client
2. **HeroUIProvider** - HeroUI theme provider
3. **DisclosureProvider** - Modal disclosure context
4. **ToastProvider** - Toast notifications

---

### DisclosureProvider

**Location:** `src/providers/DisclosureProvider/`

**Mục đích:** Quản lý state của các modals (login, register, forgot password) thông qua React Context.

**API:**
```typescript
const { isOpen, onOpen, onClose } = useLoginDiscloresure()
```

---

## 🎨 STYLING & THEMING

### Tailwind CSS 4

**Config:** `tailwind.config.js`

**Theme:**
- Coffee theme (`coffee-theme`)
- Custom colors: primary, secondary, success, warning, danger
- Font: Geist Sans, Geist Mono

**Usage:**
```typescript
className="bg-background text-foreground"
```

---

### Styled Components

**Location:** `src/components/styled/`

Một số components sử dụng `styled-components` để tạo custom styling:

**Example:**
```typescript
export const ButtonStyled = styled(Button)`
  // Custom styles
`
```

---

### Global Styles

**File:** `src/app/globals.css`

**Nội dung:**
- Tailwind directives
- CSS variables cho theme
- Global resets
- Custom utility classes

---

## 🔌 API INTEGRATION

### Axios Setup

**Client-side:** `src/utils/axios.ts`
- Base URL configuration
- Request interceptors (thêm token)
- Response interceptors (error handling)

**Server-side:** `src/utils/axiosServer.ts`
- Server-side axios instance
- Cookie-based authentication

---

### API Services

**Pattern:** Feature-based services

**Example:**
```typescript
// src/features/auth/services/authApi.ts
export const authApi = {
  login: (payload: LoginPayload) => axios.post('/auth/login', payload),
  // ...
}
```

---

### React Query Integration

**Hook:** `useClientFetch` (có thể tạo trong `src/lib/swr-fetch.tsx` hoặc tương tự)

**Usage:**
```typescript
const { data, isLoading, handleFilter, handlePaginationChange } = 
  useClientFetch('/api/endpoint')
```

---

## 📄 DANH SÁCH CÁC TRANG HIỆN TẠI

### 1. **Trang Chủ (`/`)**
- **File:** `src/app/page.tsx`
- **Mô tả:** Trang demo UI components
- **Tính năng:**
  - Demo buttons với các variants
  - Demo card component
  - Demo input fields

---

### 2. **Trang Home (`/home`)**
- **File:** `src/app/(public)/home/page.tsx`
- **Mô tả:** Trang home chính thức
- **Component:** `HomeView` từ `features/home`
- **Tính năng:**
  - Banner section
  - Navigation bar
  - Login/Profile dropdown

---

### 3. **Trang Demo (`/demo`)**
- **File:** `src/app/demo/page.tsx`
- **Mô tả:** Trang demo để test components
- **Tính năng:** Tương tự trang chủ

---

## 🔮 CÁC TRANG CÓ THỂ PHÁT TRIỂN TRONG TƯƠNG LAI

Dựa trên cấu trúc dự án, các trang có thể được thêm:

### Public Pages
- `/menu` - Menu sản phẩm
- `/about` - Giới thiệu
- `/contact` - Liên hệ
- `/register` - Đăng ký

### Protected Pages (Cần authentication)
- `/profile` - Thông tin cá nhân
- `/orders` - Lịch sử đơn hàng
- `/cart` - Giỏ hàng
- `/checkout` - Thanh toán

### Admin Pages (Cần admin role)
- `/admin/dashboard` - Dashboard quản lý
- `/admin/products` - Quản lý sản phẩm
- `/admin/orders` - Quản lý đơn hàng
- `/admin/users` - Quản lý người dùng

---

## 🔄 LUỒNG ĐIỂN HÌNH CỦA NGƯỜI DÙNG

### Luồng 1: Người dùng chưa đăng nhập

```
1. Truy cập trang chủ (/)
   └── Hiển thị Navbar với "Login" button

2. Click "Login"
   └── Mở LoginModal

3. Nhập thông tin và submit
   └── useLogin hook → authApi.login()
   └── Success → Lưu token → Đóng modal → Navbar hiển thị ProfileDropdown

4. Navigate đến /home
   └── Hiển thị HomeView với Banner
```

---

### Luồng 2: Người dùng đã đăng nhập

```
1. Truy cập bất kỳ trang nào
   └── Navbar hiển thị ProfileDropdown (thay vì Login button)

2. Click ProfileDropdown
   └── Hiển thị menu: Profile, Orders, Logout

3. Click "Logout"
   └── clearToken() → Navbar hiển thị lại "Login" button
```

---

## 🛠️ CÁC TÍNH NĂNG ĐÃ IMPLEMENT

✅ **Đã hoàn thành:**
- [x] Next.js App Router setup
- [x] Root layout với providers
- [x] Navbar component với responsive design
- [x] Login modal với form validation
- [x] Token management với Zustand
- [x] React Query integration
- [x] Styled components library
- [x] Theme configuration (Coffee theme)
- [x] Modal disclosure management
- [x] Toast notifications
- [x] Profile dropdown (basic)

---

## 📝 GHI CHÚ QUAN TRỌNG

1. **Feature-based Organization:** Code được tổ chức theo features (auth, home) thay vì theo loại (components, pages)

2. **Singleton Hooks:** Sử dụng singleton pattern cho stores và disclosures để đảm bảo single source of truth

3. **Client Hydration:** Sử dụng `ClientHydration` component để tránh hydration mismatch

4. **Type Safety:** Tất cả code đều có TypeScript types

5. **Reusable Components:** Styled components được tạo để tái sử dụng và maintain consistency

---

## 📚 TÀI LIỆU THAM KHẢO

- [Next.js Documentation](https://nextjs.org/docs)
- [HeroUI Documentation](https://heroui.com)
- [TanStack Query](https://tanstack.com/query)
- [Zustand](https://zustand-demo.pmnd.rs/)
- [Tailwind CSS](https://tailwindcss.com)

---

**Tài liệu được tạo:** 2026-01-26  
**Phiên bản:** 1.0  
**Framework:** Next.js 16.1.4 với App Router
