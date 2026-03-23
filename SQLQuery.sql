/* =========================================================
   CSDL: WEBSITE BAN DO GIA DUNG NHA BEP ONLINE (SQL Server)
   Ten bang/cot: Tieng Viet khong dau
   ========================================================= */

IF DB_ID(N'QLGiaDungBepOnline') IS NULL
BEGIN
    CREATE DATABASE QLGiaDungBepOnline;
END
GO
USE QLGiaDungBepOnline;
GO

/* =========================
   1) TAI KHOAN - VAI TRO
   ========================= */

CREATE TABLE dbo.VaiTro (
    VaiTroID        INT IDENTITY(1,1) PRIMARY KEY,
    MaVaiTro        VARCHAR(50) NOT NULL UNIQUE,   -- ADMIN, QUAN_LY, THU_NGAN, THU_KHO, KHACH_HANG
    TenVaiTro       NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.TaiKhoan (
    TaiKhoanID          INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap         VARCHAR(50) NOT NULL UNIQUE,
    MatKhauHash         VARCHAR(255) NOT NULL,
    Email               VARCHAR(120) NULL,
    SoDienThoai         VARCHAR(20) NULL,
    DangHoatDong        BIT NOT NULL DEFAULT 1,
    EmailDaXacMinh       BIT NOT NULL DEFAULT 0,
    DienThoaiDaXacMinh   BIT NOT NULL DEFAULT 0,
    LanDangNhapCuoi      DATETIME2 NULL,
    NgayTao             DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat         DATETIME2 NULL
);

CREATE TABLE dbo.TaiKhoan_VaiTro (
    TaiKhoanID      INT NOT NULL,
    VaiTroID        INT NOT NULL,
    PRIMARY KEY (TaiKhoanID, VaiTroID),
    CONSTRAINT FK_TKVT_TaiKhoan FOREIGN KEY(TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT FK_TKVT_VaiTro   FOREIGN KEY(VaiTroID)   REFERENCES dbo.VaiTro(VaiTroID)
);

-- Token doi mat khau/xac minh email (website online can)
CREATE TABLE dbo.TokenBaoMat (
    TokenID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID      INT NOT NULL,
    LoaiToken       VARCHAR(20) NOT NULL, -- RESET_MK, XAC_MINH_EMAIL
    TokenHash       VARCHAR(255) NOT NULL,
    HetHanLuc       DATETIME2 NOT NULL,
    DaDung          BIT NOT NULL DEFAULT 0,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Token_TaiKhoan FOREIGN KEY(TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT CK_Token_Loai CHECK (LoaiToken IN ('RESET_MK','XAC_MINH_EMAIL'))
);
GO

/* =========================
   2) KHACH HANG - DIA CHI
   ========================= */

CREATE TABLE dbo.KhachHang (
    KhachHangID     INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID      INT NULL UNIQUE, -- khach co the co tai khoan dang nhap
    MaKhachHang     VARCHAR(30) NOT NULL UNIQUE,
    HoTen           NVARCHAR(150) NOT NULL,
    SoDienThoai     VARCHAR(20) NOT NULL,
    Email           VARCHAR(120) NULL,
    GioiTinh        NVARCHAR(10) NULL,
    NgaySinh        DATE NULL,
    GhiChu          NVARCHAR(255) NULL,
    DangHoatDong    BIT NOT NULL DEFAULT 1,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_KH_TaiKhoan FOREIGN KEY(TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);

CREATE TABLE dbo.DiaChiKhachHang (
    DiaChiID        INT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID     INT NOT NULL,
    TenNguoiNhan    NVARCHAR(150) NOT NULL,
    SDTNguoiNhan    VARCHAR(20) NOT NULL,
    DiaChiCuThe     NVARCHAR(255) NOT NULL,
    PhuongXa        NVARCHAR(100) NULL,
    QuanHuyen       NVARCHAR(100) NULL,
    TinhThanh       NVARCHAR(100) NULL,
    MacDinh         BIT NOT NULL DEFAULT 0,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DCKH_KhachHang FOREIGN KEY(KhachHangID) REFERENCES dbo.KhachHang(KhachHangID)
);
GO

/* =========================
   3) DANH MUC - THUONG HIEU - SAN PHAM - BIEN THE
   ========================= */

CREATE TABLE dbo.DanhMuc (
    DanhMucID       INT IDENTITY(1,1) PRIMARY KEY,
    DanhMucChaID    INT NULL,
    TenDanhMuc      NVARCHAR(150) NOT NULL,
    Slug            VARCHAR(200) NOT NULL UNIQUE,
    DangHoatDong    BIT NOT NULL DEFAULT 1,
    ThuTu           INT NOT NULL DEFAULT 0,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DanhMuc_Cha FOREIGN KEY(DanhMucChaID) REFERENCES dbo.DanhMuc(DanhMucID)
);
CREATE INDEX IX_DanhMuc_Cha ON dbo.DanhMuc(DanhMucChaID);

CREATE TABLE dbo.ThuongHieu (
    ThuongHieuID    INT IDENTITY(1,1) PRIMARY KEY,
    TenThuongHieu   NVARCHAR(150) NOT NULL UNIQUE,
    QuocGia         NVARCHAR(100) NULL,
    DangHoatDong    BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.DonViTinh (
    DonViTinhID     INT IDENTITY(1,1) PRIMARY KEY,
    TenDonViTinh    NVARCHAR(50) NOT NULL UNIQUE,
    KyHieu          NVARCHAR(20) NULL
);

CREATE TABLE dbo.SanPham (
    SanPhamID       INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham       VARCHAR(30) NOT NULL UNIQUE,
    TenSanPham      NVARCHAR(200) NOT NULL,
    Slug            VARCHAR(220) NOT NULL UNIQUE,         -- SEO URL
    DanhMucID       INT NOT NULL,
    ThuongHieuID    INT NULL,
    DonViTinhID     INT NOT NULL,
    MoTaNgan        NVARCHAR(500) NULL,
    MoTaChiTiet     NVARCHAR(MAX) NULL,
    AnhDaiDien      NVARCHAR(500) NULL,
    DangHoatDong    BIT NOT NULL DEFAULT 1,
    HienThiTrangChu BIT NOT NULL DEFAULT 0,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat     DATETIME2 NULL,
    CONSTRAINT FK_SP_DanhMuc FOREIGN KEY(DanhMucID) REFERENCES dbo.DanhMuc(DanhMucID),
    CONSTRAINT FK_SP_ThuongHieu FOREIGN KEY(ThuongHieuID) REFERENCES dbo.ThuongHieu(ThuongHieuID),
    CONSTRAINT FK_SP_DonViTinh FOREIGN KEY(DonViTinhID) REFERENCES dbo.DonViTinh(DonViTinhID)
);
CREATE INDEX IX_SP_DanhMuc ON dbo.SanPham(DanhMucID);

-- SKU (bien the) la don vi ban thuc te
CREATE TABLE dbo.BienTheSanPham (
    BienTheID       INT IDENTITY(1,1) PRIMARY KEY,
    SanPhamID       INT NOT NULL,
    SKU             VARCHAR(60) NOT NULL UNIQUE,
    MaVach          VARCHAR(60) NULL UNIQUE,
    TenBienThe      NVARCHAR(200) NULL,      -- VD: 24cm - Den
    ThuocTinhJson   NVARCHAR(MAX) NULL,      -- VD: {"size":"24cm","mau":"Den"}
    GiaNhapThamChieu DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiaBan          DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiaKhuyenMai    DECIMAL(18,2) NULL,      -- neu muon set gia sale truc tiep
    SoLuongToiThieu INT NOT NULL DEFAULT 1,
    TrongLuongGram  INT NULL,
    DangHoatDong    BIT NOT NULL DEFAULT 1,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_BT_SP FOREIGN KEY(SanPhamID) REFERENCES dbo.SanPham(SanPhamID),
    CONSTRAINT CK_BT_Gia CHECK (GiaNhapThamChieu>=0 AND GiaBan>=0 AND (GiaKhuyenMai IS NULL OR GiaKhuyenMai>=0))
);
CREATE INDEX IX_BT_SP ON dbo.BienTheSanPham(SanPhamID);

CREATE TABLE dbo.HinhAnhSanPham (
    HinhAnhID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    BienTheID       INT NOT NULL,
    DuongDan        NVARCHAR(500) NOT NULL,
    LaChinh         BIT NOT NULL DEFAULT 0,
    ThuTu           INT NOT NULL DEFAULT 0,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_HA_BT FOREIGN KEY(BienTheID) REFERENCES dbo.BienTheSanPham(BienTheID)
);
GO

/* =========================
   4) KHO - TON KHO (ONLINE CAN RESERVE)
   ========================= */

CREATE TABLE dbo.Kho (
    KhoID           INT IDENTITY(1,1) PRIMARY KEY,
    MaKho           VARCHAR(30) NOT NULL UNIQUE,
    TenKho          NVARCHAR(150) NOT NULL,
    DiaChi          NVARCHAR(255) NULL,
    DangHoatDong    BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.TonKho (
    KhoID           INT NOT NULL,
    BienTheID       INT NOT NULL,
    SoLuongTon      INT NOT NULL DEFAULT 0,
    SoLuongGiuCho   INT NOT NULL DEFAULT 0, -- giu cho cho don chua thanh toan/xuat kho
    MucDatHangLai   INT NOT NULL DEFAULT 0,
    NgayCapNhat     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    PRIMARY KEY (KhoID, BienTheID),
    CONSTRAINT FK_TK_Kho FOREIGN KEY(KhoID) REFERENCES dbo.Kho(KhoID),
    CONSTRAINT FK_TK_BT  FOREIGN KEY(BienTheID) REFERENCES dbo.BienTheSanPham(BienTheID),
    CONSTRAINT CK_TK_SL CHECK (SoLuongTon>=0 AND SoLuongGiuCho>=0)
);

CREATE TABLE dbo.NhatKyKho (
    NhatKyKhoID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaNhatKy        VARCHAR(40) NOT NULL UNIQUE,
    KhoID           INT NOT NULL,
    BienTheID       INT NOT NULL,
    LoaiPhatSinh    VARCHAR(20) NOT NULL, -- NHAP, XUAT, DIEU_CHINH, GIU_CHO, HUY_GIU
    SoLuong         INT NOT NULL,
    LoaiThamChieu   VARCHAR(30) NULL,     -- DON_HANG, TRA_HANG, PHIEU_NHAP...
    ThamChieuID     BIGINT NULL,
    GhiChu          NVARCHAR(255) NULL,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_NKK_Kho FOREIGN KEY(KhoID) REFERENCES dbo.Kho(KhoID),
    CONSTRAINT FK_NKK_BT FOREIGN KEY(BienTheID) REFERENCES dbo.BienTheSanPham(BienTheID),
    CONSTRAINT CK_NKK_SL CHECK (SoLuong > 0),
    CONSTRAINT CK_NKK_Loai CHECK (LoaiPhatSinh IN ('NHAP','XUAT','DIEU_CHINH','GIU_CHO','HUY_GIU'))
);
CREATE INDEX IX_NKK_Kho_BT ON dbo.NhatKyKho(KhoID, BienTheID, NgayTao DESC);
GO

/* =========================
   5) GIO HANG - WISHLIST (ONLINE BAT BUOC)
   ========================= */

CREATE TABLE dbo.GioHang (
    GioHangID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID      INT NULL,             -- dang nhap
    KhachHangID     INT NULL,             -- neu muon gan profile khach
    MaPhien         VARCHAR(80) NULL,      -- khach vang lai (cookie/session)
    TrangThai       VARCHAR(20) NOT NULL DEFAULT 'DANG_MUA', -- DANG_MUA, DA_CHUYEN_THANH_DON
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat     DATETIME2 NULL,
    CONSTRAINT FK_GH_TK FOREIGN KEY(TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT FK_GH_KH FOREIGN KEY(KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT CK_GH_TrangThai CHECK (TrangThai IN ('DANG_MUA','DA_CHUYEN_THANH_DON'))
);

CREATE TABLE dbo.ChiTietGioHang (
    ChiTietGioHangID BIGINT IDENTITY(1,1) PRIMARY KEY,
    GioHangID       BIGINT NOT NULL,
    BienTheID       INT NOT NULL,
    SoLuong         INT NOT NULL,
    DonGiaTaiThoiDiem DECIMAL(18,2) NOT NULL DEFAULT 0, -- luu gia de tinh nhanh
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_CTGH_GH FOREIGN KEY(GioHangID) REFERENCES dbo.GioHang(GioHangID),
    CONSTRAINT FK_CTGH_BT FOREIGN KEY(BienTheID) REFERENCES dbo.BienTheSanPham(BienTheID),
    CONSTRAINT CK_CTGH_SL CHECK (SoLuong > 0)
);
CREATE INDEX IX_CTGH_GioHang ON dbo.ChiTietGioHang(GioHangID);

CREATE TABLE dbo.SanPhamYeuThich (
    KhachHangID     INT NOT NULL,
    SanPhamID       INT NOT NULL,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    PRIMARY KEY (KhachHangID, SanPhamID),
    CONSTRAINT FK_YT_KH FOREIGN KEY(KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_YT_SP FOREIGN KEY(SanPhamID) REFERENCES dbo.SanPham(SanPhamID)
);
GO

/* =========================
   6) DON HANG ONLINE - DIA CHI SNAPSHOT - LICH SU TRANG THAI
   ========================= */

CREATE TABLE dbo.PhuongThucThanhToan (
    PhuongThucID    INT IDENTITY(1,1) PRIMARY KEY,
    MaPhuongThuc    VARCHAR(30) NOT NULL UNIQUE, -- COD, CHUYEN_KHOAN, VNPAY, MOMO...
    TenPhuongThuc   NVARCHAR(100) NOT NULL,
    DangHoatDong    BIT NOT NULL DEFAULT 1
);

CREATE TABLE dbo.DonHang (
    DonHangID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang       VARCHAR(40) NOT NULL UNIQUE,
    KhachHangID     INT NULL,
    TaiKhoanID      INT NULL, -- neu checkout khi dang nhap
    KhoID           INT NOT NULL, -- kho xu ly
    TrangThai       VARCHAR(25) NOT NULL DEFAULT 'CHO_XAC_NHAN',
    -- CHO_XAC_NHAN, DA_XAC_NHAN, DANG_DONG_GOI, DANG_GIAO, DA_GIAO, HOAN_TAT, HUY
    KenhBan         VARCHAR(15) NOT NULL DEFAULT 'WEB', -- WEB, MOBILE
    TamTinh         DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiamGia         DECIMAL(18,2) NOT NULL DEFAULT 0,
    PhiVanChuyen    DECIMAL(18,2) NOT NULL DEFAULT 0,
    Thue            DECIMAL(18,2) NOT NULL DEFAULT 0,
    TongTien        DECIMAL(18,2) NOT NULL DEFAULT 0,
    GhiChu          NVARCHAR(255) NULL,
    NgayDat         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat     DATETIME2 NULL,
    CONSTRAINT FK_DH_KH FOREIGN KEY(KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_DH_TK FOREIGN KEY(TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID),
    CONSTRAINT FK_DH_Kho FOREIGN KEY(KhoID) REFERENCES dbo.Kho(KhoID),
    CONSTRAINT CK_DH_TrangThai CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_DONG_GOI','DANG_GIAO','DA_GIAO','HOAN_TAT','HUY')),
    CONSTRAINT CK_DH_Tien CHECK (TamTinh>=0 AND GiamGia>=0 AND PhiVanChuyen>=0 AND Thue>=0 AND TongTien>=0),
    CONSTRAINT CK_DH_Kenh CHECK (KenhBan IN ('WEB','MOBILE'))
);
CREATE INDEX IX_DH_NgayDat ON dbo.DonHang(NgayDat DESC);

CREATE TABLE dbo.ChiTietDonHang (
    ChiTietDonHangID BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonHangID       BIGINT NOT NULL,
    BienTheID       INT NOT NULL,
    TenSanPhamLuu   NVARCHAR(200) NOT NULL, -- snapshot
    SKU             VARCHAR(60) NOT NULL,   -- snapshot
    SoLuong         INT NOT NULL,
    DonGia          DECIMAL(18,2) NOT NULL,
    GiamGiaDong     DECIMAL(18,2) NOT NULL DEFAULT 0,
    ThanhTien       AS ((SoLuong * DonGia) - GiamGiaDong) PERSISTED,
    CONSTRAINT FK_CTDH_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID),
    CONSTRAINT FK_CTDH_BT FOREIGN KEY(BienTheID) REFERENCES dbo.BienTheSanPham(BienTheID),
    CONSTRAINT CK_CTDH_SL CHECK (SoLuong>0),
    CONSTRAINT CK_CTDH_Gia CHECK (DonGia>=0 AND GiamGiaDong>=0)
);
CREATE INDEX IX_CTDH_DonHang ON dbo.ChiTietDonHang(DonHangID);

-- Luu dia chi giao theo don (snapshot, de sau khach sua dia chi khong anh huong don cu)
CREATE TABLE dbo.DonHang_DiaChiGiao (
    DonHangID       BIGINT PRIMARY KEY,
    TenNguoiNhan    NVARCHAR(150) NOT NULL,
    SDTNguoiNhan    VARCHAR(20) NOT NULL,
    DiaChiCuThe     NVARCHAR(255) NOT NULL,
    PhuongXa        NVARCHAR(100) NULL,
    QuanHuyen       NVARCHAR(100) NULL,
    TinhThanh       NVARCHAR(100) NULL,
    GhiChuGiao      NVARCHAR(255) NULL,
    CONSTRAINT FK_DHDC_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID)
);

-- Lich su trang thai don hang (online rat can)
CREATE TABLE dbo.LichSuTrangThaiDonHang (
    LichSuID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonHangID       BIGINT NOT NULL,
    TrangThaiCu     VARCHAR(25) NULL,
    TrangThaiMoi    VARCHAR(25) NOT NULL,
    GhiChu          NVARCHAR(255) NULL,
    NguoiThucHienID INT NULL, -- TaiKhoanID hoac NhanVienID (neu ban co NhanVien)
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_LS_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID)
);
GO

/* =========================
   7) THANH TOAN ONLINE - CONG THANH TOAN - HOAN TIEN
   ========================= */

-- Thanh toan co the nhieu lan (coc + thanh toan not)
CREATE TABLE dbo.ThanhToan (
    ThanhToanID     BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonHangID       BIGINT NOT NULL,
    PhuongThucID    INT NOT NULL,
    SoTien          DECIMAL(18,2) NOT NULL,
    TrangThai       VARCHAR(20) NOT NULL DEFAULT 'CHO_THANH_TOAN',
    -- CHO_THANH_TOAN, THANH_CONG, THAT_BAI, HUY
    MaGiaoDich      VARCHAR(80) NULL, -- gateway transaction id
    NgayThanhToan   DATETIME2 NULL,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_TT_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID),
    CONSTRAINT FK_TT_PT FOREIGN KEY(PhuongThucID) REFERENCES dbo.PhuongThucThanhToan(PhuongThucID),
    CONSTRAINT CK_TT_SoTien CHECK (SoTien > 0),
    CONSTRAINT CK_TT_TrangThai CHECK (TrangThai IN ('CHO_THANH_TOAN','THANH_CONG','THAT_BAI','HUY'))
);
CREATE INDEX IX_TT_DonHang ON dbo.ThanhToan(DonHangID);

-- log callback tu cong thanh toan (VNPay/MoMo/PayPal...)
CREATE TABLE dbo.NhatKyCongThanhToan (
    NhatKyID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    ThanhToanID     BIGINT NOT NULL,
    CongThanhToan   VARCHAR(30) NOT NULL, -- VNPAY, MOMO, PAYPAL...
    DuLieuNhan      NVARCHAR(MAX) NOT NULL,
    MaKetQua        VARCHAR(30) NULL,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_NKCTT_TT FOREIGN KEY(ThanhToanID) REFERENCES dbo.ThanhToan(ThanhToanID)
);

CREATE TABLE dbo.HoanTien (
    HoanTienID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    ThanhToanID     BIGINT NOT NULL,
    SoTienHoan      DECIMAL(18,2) NOT NULL,
    TrangThai       VARCHAR(20) NOT NULL DEFAULT 'CHO_HOAN',
    -- CHO_HOAN, DA_HOAN, THAT_BAI
    LyDo            NVARCHAR(255) NULL,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_HT_TT FOREIGN KEY(ThanhToanID) REFERENCES dbo.ThanhToan(ThanhToanID),
    CONSTRAINT CK_HT_SoTien CHECK (SoTienHoan > 0),
    CONSTRAINT CK_HT_TrangThai CHECK (TrangThai IN ('CHO_HOAN','DA_HOAN','THAT_BAI'))
);
GO

/* =========================
   8) GIAO HANG (VAN DON)
   ========================= */

CREATE TABLE dbo.GiaoHang (
    GiaoHangID      BIGINT IDENTITY(1,1) PRIMARY KEY,
    DonHangID       BIGINT NOT NULL UNIQUE,
    DonViVanChuyen  NVARCHAR(120) NULL,
    MaVanDon        VARCHAR(80) NULL,
    TrangThaiGiao   VARCHAR(20) NOT NULL DEFAULT 'SAN_SANG',
    -- SAN_SANG, DA_LAY, DANG_GIAO, DA_GIAO, THAT_BAI, HOAN
    PhiVanChuyen    DECIMAL(18,2) NOT NULL DEFAULT 0,
    NgayGui         DATETIME2 NULL,
    NgayGiao        DATETIME2 NULL,
    GhiChu          NVARCHAR(255) NULL,
    CONSTRAINT FK_GH_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID),
    CONSTRAINT CK_GH_TrangThai CHECK (TrangThaiGiao IN ('SAN_SANG','DA_LAY','DANG_GIAO','DA_GIAO','THAT_BAI','HOAN')),
    CONSTRAINT CK_GH_Phi CHECK (PhiVanChuyen >= 0)
);
GO

/* =========================
   9) TRA HANG / DOI HANG (ONLINE)
   ========================= */

CREATE TABLE dbo.TraHang (
    TraHangID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaTraHang       VARCHAR(40) NOT NULL UNIQUE,
    DonHangID       BIGINT NOT NULL,
    TrangThai       VARCHAR(20) NOT NULL DEFAULT 'YEU_CAU',
    -- YEU_CAU, DA_DUYET, TU_CHOI, DANG_XU_LY, HOAN_TAT
    LyDo            NVARCHAR(255) NULL,
    SoTienDuKienHoan DECIMAL(18,2) NOT NULL DEFAULT 0,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_TH_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID),
    CONSTRAINT CK_TH_TrangThai CHECK (TrangThai IN ('YEU_CAU','DA_DUYET','TU_CHOI','DANG_XU_LY','HOAN_TAT')),
    CONSTRAINT CK_TH_Tien CHECK (SoTienDuKienHoan >= 0)
);

CREATE TABLE dbo.ChiTietTraHang (
    ChiTietTraHangID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TraHangID       BIGINT NOT NULL,
    BienTheID       INT NOT NULL,
    SoLuong         INT NOT NULL,
    TinhTrangHang   NVARCHAR(255) NULL,
    DonGiaHoan      DECIMAL(18,2) NOT NULL DEFAULT 0,
    ThanhTienHoan   AS (SoLuong * DonGiaHoan) PERSISTED,
    CONSTRAINT FK_CTTH_TH FOREIGN KEY(TraHangID) REFERENCES dbo.TraHang(TraHangID),
    CONSTRAINT FK_CTTH_BT FOREIGN KEY(BienTheID) REFERENCES dbo.BienTheSanPham(BienTheID),
    CONSTRAINT CK_CTTH_SL CHECK (SoLuong>0),
    CONSTRAINT CK_CTTH_DonGia CHECK (DonGiaHoan>=0)
);
GO

/* =========================
   10) KHUYEN MAI - MA GIAM GIA
   ========================= */

CREATE TABLE dbo.MaGiamGia (
    MaGiamGiaID     INT IDENTITY(1,1) PRIMARY KEY,
    Code            VARCHAR(40) NOT NULL UNIQUE,
    TenMa           NVARCHAR(200) NOT NULL,
    LoaiGiam        VARCHAR(10) NOT NULL, -- PHAN_TRAM, SO_TIEN
    GiaTriGiam      DECIMAL(18,2) NOT NULL,
    BatDau          DATETIME2 NOT NULL,
    KetThuc         DATETIME2 NOT NULL,
    DonHangToiThieu DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiamToiDa       DECIMAL(18,2) NULL,
    GioiHanLuotDung INT NULL,            -- tong
    GioiHanMoiKhach INT NULL,            -- moi khach
    DaDung          INT NOT NULL DEFAULT 0,
    DangHoatDong    BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_MGG_Loai CHECK (LoaiGiam IN ('PHAN_TRAM','SO_TIEN')),
    CONSTRAINT CK_MGG_GiaTri CHECK (GiaTriGiam>=0),
    CONSTRAINT CK_MGG_ThoiGian CHECK (KetThuc > BatDau)
);

CREATE TABLE dbo.SuDungMaGiamGia (
    SuDungID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaGiamGiaID     INT NOT NULL,
    DonHangID       BIGINT NOT NULL UNIQUE,
    KhachHangID     INT NULL,
    NgaySuDung      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_SDMGG_Ma FOREIGN KEY(MaGiamGiaID) REFERENCES dbo.MaGiamGia(MaGiamGiaID),
    CONSTRAINT FK_SDMGG_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID),
    CONSTRAINT FK_SDMGG_KH FOREIGN KEY(KhachHangID) REFERENCES dbo.KhachHang(KhachHangID)
);
GO

/* =========================
   11) DANH GIA - BINH LUAN (ONLINE RAT CAN)
   ========================= */

CREATE TABLE dbo.DanhGiaSanPham (
    DanhGiaID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    SanPhamID       INT NOT NULL,
    KhachHangID     INT NULL,
    SoSao           TINYINT NOT NULL,          -- 1..5
    TieuDe          NVARCHAR(200) NULL,
    NoiDung         NVARCHAR(2000) NULL,
    HienThi         BIT NOT NULL DEFAULT 1,
    DonHangID       BIGINT NULL,              -- neu muon chi cho review sau khi mua
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_DG_SP FOREIGN KEY(SanPhamID) REFERENCES dbo.SanPham(SanPhamID),
    CONSTRAINT FK_DG_KH FOREIGN KEY(KhachHangID) REFERENCES dbo.KhachHang(KhachHangID),
    CONSTRAINT FK_DG_DH FOREIGN KEY(DonHangID) REFERENCES dbo.DonHang(DonHangID),
    CONSTRAINT CK_DG_SoSao CHECK (SoSao BETWEEN 1 AND 5)
);
CREATE INDEX IX_DG_SP ON dbo.DanhGiaSanPham(SanPhamID, NgayTao DESC);
GO

/* =========================
   12) CMS: BANNER - TRANG TINH (GIOI THIEU/CHINH SACH)
   ========================= */

CREATE TABLE dbo.Banner (
    BannerID        INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe          NVARCHAR(200) NULL,
    Anh             NVARCHAR(500) NOT NULL,
    Link            NVARCHAR(500) NULL,
    Text   	    NVARCHAR(200)
    ThuTu  	    INT NOT NULL DEFAULT 0,
    IsActive        BIT NOT NULL DEFAULT 1

  );

CREATE TABLE dbo.TrangNoiDung (
    TrangID         INT IDENTITY(1,1) PRIMARY KEY,
    MaTrang         VARCHAR(60) NOT NULL UNIQUE,   -- gioi-thieu, chinh-sach-doi-tra...
    TieuDe          NVARCHAR(200) NOT NULL,
    NoiDung         NVARCHAR(MAX) NOT NULL,
    DangHienThi     BIT NOT NULL DEFAULT 1,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayCapNhat     DATETIME2 NULL
);
GO

/* =========================
   13) NHAT KY HE THONG
   ========================= */

CREATE TABLE dbo.NhatKyHeThong (
    NhatKyID        BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID      INT NULL,
    HanhDong        VARCHAR(50) NOT NULL,      -- INSERT/UPDATE/DELETE/LOGIN...
    TenBang         VARCHAR(100) NOT NULL,
    KhoaBanGhi      VARCHAR(100) NULL,
    DuLieuCu        NVARCHAR(MAX) NULL,
    DuLieuMoi       NVARCHAR(MAX) NULL,
    DiaChiIP        VARCHAR(45) NULL,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_NKHT_TK FOREIGN KEY(TaiKhoanID) REFERENCES dbo.TaiKhoan(TaiKhoanID)
);
GO

/* =========================
   14) VIEW BAO CAO CO BAN
   ========================= */

CREATE OR ALTER VIEW dbo.vw_DoanhThuTheoNgay
AS
SELECT
    CAST(NgayDat AS DATE) AS Ngay,
    COUNT(*) AS SoDon,
    SUM(TongTien) AS DoanhThu
FROM dbo.DonHang
WHERE TrangThai IN ('HOAN_TAT','DA_GIAO')
GROUP BY CAST(NgayDat AS DATE);
GO

CREATE OR ALTER VIEW dbo.vw_TopBanChay
AS
SELECT TOP 100
    bt.BienTheID,
    bt.SKU,
    sp.TenSanPham,
    SUM(ct.SoLuong) AS TongSoLuong,
    SUM(ct.ThanhTien) AS TongTien
FROM dbo.ChiTietDonHang ct
JOIN dbo.DonHang dh ON dh.DonHangID = ct.DonHangID
JOIN dbo.BienTheSanPham bt ON bt.BienTheID = ct.BienTheID
JOIN dbo.SanPham sp ON sp.SanPhamID = bt.SanPhamID
WHERE dh.TrangThai IN ('HOAN_TAT','DA_GIAO')
GROUP BY bt.BienTheID, bt.SKU, sp.TenSanPham
ORDER BY TongSoLuong DESC;
GO



