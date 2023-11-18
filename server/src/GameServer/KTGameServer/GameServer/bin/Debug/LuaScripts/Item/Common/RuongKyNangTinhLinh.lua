-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongKyNangTinhLinh = Scripts[200106]
local IDSkill = {
	[8415] = { --Rương kỹ năng tinh linh cấp 10
		 { ItemID = 34455, name = " Sách kỹ năng Kim Long Quyết ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34458, name = " Sách kỹ năng Âm Độc Chưởng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34461, name = " Sách kỹ năng Âm Độc Cuồng Đao ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34471, name = " Sách kỹ năng Li Hỏa Đạn ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34479, name = " Sách kỹ năng Hàn Băng Quyết ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34481, name = " Sách kỹ năng Hàn Băng Chi Thủ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34484, name = " Sách kỹ năng Thiên Ngoại Hàn Băng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34488, name = " Sách kỹ năng Phi Tuyết Phiêu Hoa ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34492, name = " Sách kỹ năng Liệt Hỏa Thiêu Thân ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34494, name = " Sách kỹ năng Phần Hoa Lưu Thủ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34495, name = " Sách kỹ năng Liệm Diệm Cuồng Trảm ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34508, name = " Sách kỹ năng Liệt Diệm Âm Hỏa ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34509, name = " Sách kỹ năng Cuồng Phong Thuật ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34510, name = " Sách kỹ năng Cuồng Lôi Sát Kích ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34518, name = " Sách kỹ năng Ám Lôi Chỉ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34523, name = " Sách kỹ năng Băng Tinh Nhật Ảnh ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34524, name = " Sách kỹ năng Băng Lam Khí Công ", Number = 1, Series = -1, Bond = 1 },
	},
	[8416] = { --Rương kỹ năng tinh linh cấp 30
		 { ItemID = 34472, name = " Sách kỹ năng Ám Độc Tiễn ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34459, name = " Sách kỹ năng Khô Lâu Độc Thủ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34462, name = " Sách kỹ năng U Hồn Vạn Độc ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34475, name = " Sách kỹ năng Bách Hoa Lạc Trần ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34480, name = " Sách kỹ năng Nguyệt Ảnh Hàn Băng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34483, name = " Sách kỹ năng Phi Tuyết Phi Hoa ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34486, name = " Sách kỹ năng Băng Thiên Tuyết Địa ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34489, name = " Sách kỹ năng Lệ Hoa Lạc Trần ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34496, name = " Sách kỹ năng Địa Hỏa Tình Thiên ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34498, name = " Sách kỹ năng Bích Diệm Chi Hỏa ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34511, name = " Sách kỹ năng Thiên Lôi Tế Đàn ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34519, name = " Sách kỹ năng Thất Tinh La Sát Trận ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34521, name = " Sách kỹ năng Thiên Lôi Quyết ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34525, name = " Sách kỹ năng Băng Phong Thiến Ảnh ", Number = 1, Series = -1, Bond = 1 },
	},
	[8417] = { --Rương kỹ năng tinh linh cấp 50
		 { ItemID = 34457, name = " Sách kỹ năng Như Lai Thần Chưởng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34460, name = " Sách kỹ năng Âm Độc Hủ Cốt ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34463, name = " Sách kỹ năng Chu Mô Công ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34474, name = " Sách kỹ năng Tiểu Lý Phi Tiêu ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34476, name = " Sách kỹ năng Thiên La Chân Thức Trận ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34477, name = " Sách kỹ năng Mãn Thiên Hoa Lạc Tiêu ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34482, name = " Sách kỹ năng Nguyệt Ảnh Phi Vũ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34487, name = " Sách kỹ năng Băng Tinh Lạc Địa ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34490, name = " Sách kỹ năng Băng Lan Phiến ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34493, name = " Sách kỹ năng Long Trảo Thủ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34497, name = " Sách kỹ năng Nhược Hỏa Hấp Huyết ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34499, name = " Sách kỹ năng Hỏa Diệm Phi Mã ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34512, name = " Sách kỹ năng Ngũ Lôi Oanh Đỉnh ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34520, name = " Sách kỹ năng Võng Lôi Kiếm ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34522, name = " Sách kỹ năng Bát Quái Tam Thanh Đàn ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34526, name = " Sách kỹ năng Thiên Băng Kiếm Ảnh ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34527, name = " Sách kỹ năng Kiếm Ảnh Huyền Băng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34562, name = " Sách kỹ năng Cuồng Phong Thiên Lôi Pháp ", Number = 1, Series = -1, Bond = 1 },
	}
	, [8418] = { --Rương kỹ năng tinh linh cấp 90
		 { ItemID = 34528, name = " Sách kỹ năng Địa Không Phá Kích ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34529, name = " Sách kỹ năng Hủ Độc Thực Cốt ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34531, name = " Sách kỹ năng Thương Hạt Chi Linh ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34532, name = " Sách kỹ năng Bách Độc Ngân Xuyên Châm ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34534, name = " Sách kỹ năng Huyền Băng Thanh Phong Kiếm ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34536, name = " Sách kỹ năng Nguyệt Lạc Sương Băng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34538, name = " Sách kỹ năng Hàn Băng Ánh Tuyết ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34540, name = " Sách kỹ năng Huyền Băng Hộ Pháp ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34542, name = " Sách kỹ năng Viêm Long Xuyên Vân Tụ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34544, name = " Sách kỹ năng Liệt Hỏa Thiên Bổng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34546, name = " Sách kỹ năng Địa Hỏa Thiên Lôi ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34547, name = " Sách kỹ năng Liệt Hỏa Liên Thành ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34548, name = " Sách kỹ năng Bát Quái Địa Linh Trận ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34550, name = " Sách kỹ năng Thái Cực Huyền Thiên Kiếm ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34551, name = " Sách kỹ năng Cuồng Phong Địa Liệt ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34554, name = " Sách kỹ năng Thiên Lôi Thiên Giáng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34556, name = " Sách kỹ năng Nguyệt Ảnh Chi Thủ ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34558, name = " Sách kỹ năng Lan Tú Quang Hoàn ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34559, name = " Sách kỹ năng U Linh Huyền Âm Độc ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34560, name = " Sách kỹ năng Âm Ty Hỏa Phát Độc ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34561, name = " Sách kỹ năng Đạt Ma Quyền Pháp ", Number = 1, Series = -1, Bond = 1 },
	}
	, [8419] = { --Rương kỹ năng tinh linh Lure
		 { ItemID = 34456, name = " Sách kỹ năng Hách Không Vô Tướng ", Number = 1, Series = -1, Bond = 1 },
	}, [8420] =
	 { --Rương kỹ năng tinh linh bùa
		 { ItemID = 34464, name = " Sách kỹ năng Cuồng Lôi Ám Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34465, name = " Sách kỹ năng Liệt Diệm Ám Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34466, name = " Sách kỹ năng Hàn Băng Ám Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34467, name = " Sách kỹ năng Kim Hoàng Ám Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34468, name = " Sách kỹ năng Hủ Độc Ám Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34469, name = " Sách kỹ năng U Minh U Hồn ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34470, name = " Sách kỹ năng Bách Độc Bất Xâm ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34501, name = " Sách kỹ năng Ly Hỏa Thần Công ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34502, name = " Sách kỹ năng Huyễn Ảnh Phi Thân ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34503, name = " Sách kỹ năng Ma Viêm Chi Giáng ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34505, name = " Sách kỹ năng Viêm Nhãn Ám Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34506, name = " Sách kỹ năng Huyễn Nhật Đoạt Mệnh ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34513, name = " Sách kỹ năng Ám Lôi Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34514, name = " Sách kỹ năng Địa Lôi Sát Kích ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34516, name = " Sách kỹ năng Ngạo Tuyết Tâm Chú ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34517, name = " Sách kỹ năng Huyễn Ảnh Chân Quân ", Number = 1, Series = -1, Bond = 1 },
		 { ItemID = 34515, name = " Sách kỹ năng Nhất Khí Hộ Thể ", Number = 1, Series = -1, Bond = 1 },
	}, [8421] = {--Rương kỹ năng tinh linh cấp Bùa 90
		 { ItemID = 34507, name = " Sách kỹ năng Viêm Hỏa Chi Châu ", Number = 1, Series = -1, Bond = 1 },

	}
}
-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function RuongKyNangTinhLinh:OnPreCheckCondition(scene, item, player, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> RuongKyNangTinhLinh:OnPreCheckCondition")--
	-- ************************** --
	return true
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi để thực thi Logic khi người sử dụng vật phẩm, sau khi đã thỏa mãn hàm kiểm tra điều kiện
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function RuongKyNangTinhLinh:OnUse(scene, item, player, otherParams)

	-- ************************** --
	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	local IDItem = item:GetItemID()
	if IDSkill[IDItem] == nil then
		player:AddNotification("Vật phẩm không hợp lệ!" .. IDItem .. "")
		return false
 	end
	dialog:AddText("Dùng được kỹ năng cực phẩm. Hãy chọn kỹ năng mình cần.<br><color=#53f9e8>(Nhận Sách Kỹ Tinh Linh sẽ tự động khóa)</color>")
	
	for key, value in pairs(IDSkill[IDItem]) do
		dialog:AddSelection(key,value.name)
	end

		dialog:AddSelection(555555, "Để ta suy nghĩ đã")
		dialog:Show(item, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function RuongKyNangTinhLinh:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> RuongKyNangTinhLinh:OnSelection, selectionID = " .. selectionID)--

	local dialog = GUI.CreateItemDialog()
	local IDItem = item:GetItemID()
	if IDSkill[IDItem] == nil then
 player:AddNotification("Vật phẩm không hợp lệ!" .. IDItem .. "")
 return false
 end
	for key, value in pairs(IDSkill[IDItem]) do
		if selectionID == key then
			Player.RemoveItem(player, item:GetID())
			Player.AddItemLua(player, value.ItemID, value.Number, value.Series, value.Bond)
			player:AddNotification("Nhận "..value.name.." <color=red>thành công</color>")
			GUI.CloseDialog(player)
		else
			dialog:AddText("Lỗi không có vật phẩm")
		end
	end
	if selectionID == 555555 then
		GUI.CloseDialog(player)
	end
	-- ************************** --
	item:FinishUsing(player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
