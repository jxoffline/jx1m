--[[
	Script này được tải mặc định cùng hệ thống.
	Toàn bộ các biến, hàm bên dưới đều là Global và có thể được dùng ở mọi Script khác.
}]]
-- Danh sách đối tượng trong hệ thống
Global_ObjectTypes = {
	-- Người chơi
	OT_CLIENT = 0,
	-- Quái
	OT_MONSTER = 1,
	-- NPC
	OT_NPC = 5,
	-- Điểm thu thập
	OT_GROWPOINT = 9,
	-- Khu vực động
	OT_DYNAMIC_AREA = 10,
}

-- Dữ liệu phụ bản
Global_Params = {
	-- Bạch Hổ Đường
	BaiHuTang_BeginRegistered = false,	-- Đang mở báo danh
	BaiHuTang_Stage = 0,					-- Đang ở tầng nào
	-- End
}

-- Trạng thái PK
Global_PKMode = {
	-- Hòa bình
	Peace = 0,
	-- Phe
	Team = 1,
	-- Bang hội
	Guild = 2,
	-- Đồ sát
	All = 3,
	-- Thiện ác
	Moral = 4,
	-- Tùy chọn tùy theo thiết lập sự kiện
	Custom = 5,
}

-- ID hoạt động
Global_Events = {
	-- Bạch Hổ Đường
	BaiHuTang = 10,
}

-- Danh sách ID môn phái
Global_FactionID = {
	-- Không có
	None = 0,
	-- Thiếu Lâm
	ShaoLin = 1,
	-- Thiên Vương
	TianWang = 2,
	-- Đường Môn
	TangMen = 3,
	-- Ngũ Độc
	WuDu = 4,
	-- Nga My
	EMei = 5,
	-- Thúy Yên
	CuiYan = 6,
	-- Cái Bang
	GaiBang = 7,
	-- Thiên Nhẫn
	TianRen = 8,
	-- Võ Đang
	WuDang = 9,
	-- Côn Lôn
	KunLun = 10,
	-- Hoa Son
	HoaSon = 11,
	-- Minh Giao
	MinhGiao = 12,
}
Global_TypeMoney = {
	BacKhoa = 0,
	Bac=1,
	Dong=2,
	DongKhoa=3,
}
Global_TypeMoneyName = {
	[Global_TypeMoney.BacKhoa]=" Bạc Khóa",
	[Global_TypeMoney.Bac]=" Bạc",
	[Global_TypeMoney.Dong]=" Đồng",
	[Global_TypeMoney.DongKhoa]=" Đồng Khóa",
}

-- Chức vị trong bang
Global_GuildRank = {
  -- Bang chúng
  Member = 0,
  -- Bang chủ
  Master = 1,
  -- Phó bang chủ
  ViceMaster = 2,
  -- Trưởng lão
  Ambassador = 3,
  -- Đường chủ
  ViceAmbassador = 4,
  -- Tinh anh
  Elite = 5,
}

-- Chức vị trong tộc
Global_FamilyRank = {
	-- Thành Viên
	Member = 0,
	-- Tộc Trưởng
	Master = 1,
	-- Tộc Phó
	ViceMaster = 2,
}

Global_CAMP = {
	[101]={NameActivity="Lệnh Bài Nghĩa Quân"},
	[102]={NameActivity="Lệnh Bài Quân doanh"},
	[201]={NameActivity="Lệnh Bài Biện Kinh"},
	[202]={NameActivity="Lệnh Bài Phượng Tường"},
	[203]={NameActivity="Lệnh Bài Tương Dương"},

	[301]={NameActivity="Lệnh Bài Thiếu Lâm"},
	[302]={NameActivity="Lệnh Bài Thiên Vương"},
	[303]={NameActivity="Lệnh Bài Đường Môn"},
	[304]={NameActivity="Lệnh Bài Ngũ Độc"},
	[305]={NameActivity="Lệnh Bài Nga My"},
	[306]={NameActivity="Lệnh Bài Thúy Yên"},
	[307]={NameActivity="Lệnh Bài Cái Bang"},
	[308]={NameActivity="Lệnh Bài Thiên Nhẫn"},
	[309]={NameActivity="Lệnh Bài Võ Đang"},
	[310]={NameActivity="Lệnh Bài Côn Lôn"},
	[311]={NameActivity="Lệnh Bài Hoa Sơn"},
	
	[501]={NameActivity="Lệnh Bài Bạch Hổ-"},
	[502]={NameActivity="Lệnh Bài Phong Hỏa Liên Thành"},
	--[503]={NameActivity="Lệnh Bài Tiêu Dao Cốc"},
	--[504]={NameActivity="Lệnh Bài Chúc phúc"},
	--[505]={NameActivity="Lệnh Bài Hoạt động Thịnh Hạ 2010"},
	--[506]={NameActivity="Lệnh Bài Di tích Hàn Vũ"},
	--[507]={NameActivity="Lệnh Bài Mỹ nhân"},
	--[508]={NameActivity="Lệnh Bài VIP"},
	--[601]={NameActivity="Lệnh Bài Cao thủ (kim)"},
	--[602]={NameActivity="Lệnh Bài Cao thủ (mộc)"},
	--[603]={NameActivity="Lệnh Bài Cao thủ (thủy)"},
	--[604]={NameActivity="Lệnh Bài Cao thủ (hỏa)"},
	--[605]={NameActivity="Lệnh Bài Cao thủ (thổ)"},
	[701]={NameActivity="Lệnh Bài Võ Lâm Liên Đấu"},
	--[801]={NameActivity="Lệnh Bài Tranh Đoạt Lãnh Thổ"},
	--[901]={NameActivity="Lệnh Bài Tần Lăng-Quan Phủ"},
	--[902]={NameActivity="Lệnh Bài Tần Lăng-Phát Khâu Môn"},
	--[1001]={NameActivity="Lệnh Bài Đoàn viên dân tộc"},
	--[1101]={NameActivity="Lệnh Bài Đại Hội Võ Lâm"},
	--[1201]={NameActivity="Lệnh Bài Liên đấu liên server"},
}
-- Danh sách môn phái
Global_FactionName = {
	[Global_FactionID.None] = "Không có",
	[Global_FactionID.ShaoLin] = "Thiếu Lâm",
	[Global_FactionID.TianWang] = "Thiên Vương",
	[Global_FactionID.TangMen] = "Đường Môn",
	[Global_FactionID.WuDu] = "Ngũ Độc",
	[Global_FactionID.EMei] = "Nga My",
	[Global_FactionID.CuiYan] = "Thúy Yên",
	[Global_FactionID.GaiBang] = "Cái Bang",
	[Global_FactionID.TianRen] = "Thiên Nhẫn",
	[Global_FactionID.WuDang] = "Võ Đang",
	[Global_FactionID.KunLun] = "Côn Lôn",
	[Global_FactionID.HoaSon] = "Hoa Sơn",
	[Global_FactionID.MinhGiao] = "Minh Giáo",
}

-- danh sách các phái Truyền thống phù Item--
--secret1 = mật tịch 1 - 2;
--press = ngũ hành ấn 1-2;
Global_NameMapItemPhaiID={
	[28] = {
		ID = 28, Name = "Thiếu Lâm Phái", PosX = 5500, PosY = 3230 ,FactionID = 1, secret1 = 3232, secret2= 3233 ,press =3864
	},
	[27] = {
		ID = 27, Name = "Thiên Vương Bang", PosX = 5300, PosY = 3300 ,FactionID  =2 ,secret1 = 3234, secret2= 3235,press =3865
	},
	[9] = {
		ID = 9, Name = "Đường Môn ", PosX = 3900, PosY = 2550 ,FactionID  =3,secret1 = 3236, secret2= 3237,press =3866
	},
	[20] = {
		ID = 20, Name = "Ngũ Độc Giáo", PosX = 5340, PosY = 3090 ,FactionID = 4, secret1 = 3238, secret2= 3239,press =3867
	},
	[19] = {
		ID = 19, Name = "Nga My Phái", PosX = 3820, PosY = 3400  ,FactionID =5, secret1 = 3240, secret2= 3241,press =3868
	},
	[29] = {
		ID = 29, Name = "Thúy Yên Môn", PosX = 4470, PosY = 4280 ,FactionID =6,  secret1 = 3242, secret2= 3243,press =3869
	},
	[3] = {
		ID = 3, Name = "Cái Bang Phái", PosX = 4070, PosY = 4580 ,FactionID =7,secret1 = 3244, secret2= 3245,press =3870
	},
	[42] = {
		ID = 42, Name = "Thiên Nhẫn Giáo", PosX = 3820, PosY = 2090 ,FactionID =8,secret1 = 3246, secret2= 3247,press =3871
	},
	[33] = {
		ID = 33, Name = "Võ Đang Phái", PosX = 4760, PosY = 2820 ,FactionID =9, secret1 = 3248, secret2= 3249,press =3872
	},
	[5] = {
		ID = 5, Name = "Côn Lôn Phái", PosX = 4710, PosY = 2160 ,FactionID =10, secret1 = 3250, secret2= 3251,press =3873
	},
	[41] = {
		ID = 41, Name = "Hoa Sơn ", PosX = 3610, PosY = 6000 ,FactionID =11, secret1 = 3252, secret2= 3253,press =3874
	},
	[91] = {
		ID = 91, Name = "Minh Giao", PosX = 2600, PosY = 1760 ,FactionID =12, secret1 = 3252, secret2= 3253,press =3874,
	},
}

--danh sách thành  Truyền thống phù Item--
Global_NameMapItemThanhID ={
	[39] = {
		ID = 39, Name = "Phượng Tường", PosX = 4480, PosY = 2380
	},
	[2] = {
		ID = 2, Name = "Biện Kinh", PosX =6030, PosY = 3570
	},
	[32] = {
		ID = 32, Name = "Tương Dương", PosX = 6320, PosY = 3330
	},
}
Global_NameMapItemThonID={
	[10] = {
		ID = 10, Name = "Giang Tân Thôn", PosX = 5930, PosY = 3040
	},
	[1] = {
		ID = 1, Name = "Ba Lăng huyện", PosX = 5670, PosY = 3000
	},
}



-- danh sách các Phai Xa phu--
Global_NameMapPhaiID={
	[28] = {
		ID = 28, Name = "Thiếu Lâm Phái", PosX = 1880, PosY = 1500  
	},
	[27] = {
		ID = 27, Name = "Thiên Vương Bang", PosX = 2250, PosY = 2520 
	},
	[9] = {
		ID = 9, Name = "Đường Môn", PosX = 1500, PosY = 1870
	},
	[19] = {
		ID = 19, Name = "Nga My Phái", PosX = 2320, PosY = 2570 
	},
	[29] = {
		ID = 29, Name = "Thúy Yên Môn", PosX = 2350, PosY = 3170
	},
	[3] = {
		ID = 3, Name = "Cái Bang Phái", PosX = 3980, PosY = 8020
	},
	[33] = {
		ID = 33, Name = "Võ Đang Phái", PosX = 3100, PosY = 1990
	},
	[5] = {
		ID = 5, Name = "Côn Lôn Phái", PosX = 2750, PosY = 3090
	},
	[20] = {
		ID = 20, Name = "Ngũ Độc Giáo", PosX = 3260, PosY = 1350
	},
	[41] = {
		ID = 41, Name = "Hoa Sơn Phái", PosX = 4640, PosY = 4100
	},
	[42] = {
		ID = 42, Name = "Thiên Nhẫn Giáo", PosX = 2735, PosY = 1450
	},
	[91] = {
		ID = 91, Name = "Minh Giao", PosX = 2600, PosY = 1760
	},
}
 Global_TeleportMiddle={
	[10] = {
		ID = 10, Name = "Giang Tân Thôn", PosX = 5930, PosY = 3040
	},
	[1] = {
		ID = 1, Name = "Ba Lăng huyện", PosX = 5670, PosY = 3000
	},
	[28] = {
		ID = 28, Name = "Thiếu Lâm Phái", PosX = 1880, PosY = 1500  
	},
	[27] = {
		ID = 27, Name = "Thiên Vương Bang", PosX = 2250, PosY = 2520 
	},
	[9] = {
		ID = 9, Name = "Đường Môn", PosX = 1500, PosY = 1870
	},
	[19] = {
		ID = 19, Name = "Nga My Phái", PosX = 2320, PosY = 2570 
	},
	[29] = {
		ID = 29, Name = "Thúy Yên Môn", PosX = 2350, PosY = 3170
	},
	[3] = {
		ID = 3, Name = "Cái Bang Phái", PosX = 3980, PosY = 8020
	},
	[33] = {
		ID = 33, Name = "Võ Đang Phái", PosX = 3100, PosY = 1990
	},
	[5] = {
		ID = 5, Name = "Côn Lôn Phái", PosX = 2750, PosY = 3090
	},
	[20] = {
		ID = 20, Name = "Ngũ Độc Giáo", PosX = 3260, PosY = 1350
	},
	[41] = {
		ID = 41, Name = "Hoa Sơn Phái", PosX = 4640, PosY = 4100
	},
	[42] = {
		ID = 42, Name = "Thiên Nhẫn Giáo", PosX = 2735, PosY = 1450
	},
	[39] = {
		ID = 39, Name = "Phượng Tường", PosX = 4480, PosY = 2380
	},
	[2] = {
		ID = 2, Name = "Biện Kinh", PosX =6030, PosY = 3570
	},
	[32] = {
		ID = 32, Name = "Tương Dương", PosX = 6320, PosY = 3330
	},
	[91] = {
		ID = 91, Name = "Minh Giao", PosX = 2600, PosY = 1760
	},
}
--danh sách thành Các xa phu--
Global_NameMapThanhID ={

	[2] = {
		ID = 2, Name = "Biện Kinh", PosX = 2650, PosY = 1690  
	},
	[32] = {
		ID = 32, Name = "Tương Dương", PosX = 2570, PosY = 3280
	},
	[39] = {
		ID = 39, Name = "Phượng Tường", PosX = 6000, PosY = 1090
	},

}
-- danh sách các thôn xa phu--
Global_NameMapThonID={
	[10] = {
		ID = 10, Name = "Giang Tân Thôn", PosX = 3220, PosY = 4410  
	},
	[1] = {
		ID = 1, Name = "Ba Lăng huyện", PosX = 4450, PosY = 2100
	},
}