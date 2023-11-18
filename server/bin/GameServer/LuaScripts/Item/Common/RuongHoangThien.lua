-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongHoangThien = Scripts[200112]

-- ************************** --
local ItemRuongHoangThienNam = {
    [30980] = {id= 30980, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31070] = {id= 31070, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31162] = {id= 31162, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31252] = {id= 31252, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31344] = {id= 31344, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31524] = {id= 31524, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31436] = {id= 31436, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
}
local ItemRuongHoangThienNu = {
    [30981] = {id= 30981, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31071] = {id= 31071, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31163] = {id= 31163, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31253] = {id= 31253, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31345] = {id= 31345, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31525] = {id= 31525, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
	[31437] = {id= 31437, Time =-1 , Series=-1, Bound =1,enhanceLevel=0, Number=1 }
	,
}


-- ************************** --

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function RuongHoangThien:OnPreCheckCondition(scene, item, player, otherParams)

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
function RuongHoangThien:OnUse(scene, item, player, otherParams)

    -- ************************** --
	local dialog = GUI.CreateItemDialog()
    dialog:AddText("Bạn có chắc chắn muốn sử dụng :" .. item:GetName() .. "?")

    dialog:AddSelection(1, "Ta đồng ý")
    dialog:AddSelection(2, "Ta suy nghĩ đã")

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
function RuongHoangThien:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 2 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		GUI.CloseDialog(player)
		-- Level rương
		--local itemLevel = item:GetItemLevel()
		-- Lượng NHHT nhận được
		--local nCount = tbLevel[itemLevel]
		
		-- Toác
	--Kiểu	if not tbLevel[itemLevel] then
			--self:ShowDialog(item, player, "Có lỗi khi sử dụng vật phẩm, hãy thử lại sau!")
		--	return
	--	end
		
		-- Túi không đủ chỗ trống
		if Player.HasFreeBagSpaces(player, 15) == false then
			self:ShowDialog(item, player, string.format("Túi đồ cần để trống tối thiểu <color=green>15 ô trống</color> trong túi đồ để lấy đồ Hoang Thiên!"))
			return
		end
		-- Xóa rương
       -- public static bool AddItemLua(Lua_Player player, int ItemID, int Number, int Series, int LockStatus, int enhanceLevel = 0, int ExpriesTime = -1)
       
		Player.RemoveItem(player, item:GetID())
        if(player:GetSex() ==0) then
            for key , value in pairs (ItemRuongHoangThienNam) do
                -- Thêm đồ Hoang Thiên tương 
                Player.AddItemLua(player, key, value.Number, value.Series ,value.Bound)
            end
        else 
            for key , value in pairs (ItemRuongHoangThienNu) do
                -- Thêm đồ Hoang Thiên tương 
                Player.AddItemLua(player, key, value.Number, value.Series ,value.Bound)
            end
        end
		
		self:ShowDialog(item, player, string.format("Nhận thành công <color=green>Set Hoang Thiên!</color> "))
	end
	-- ************************** --
  
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function RuongHoangThien:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end

function RuongHoangThien:ShowDialog(item, player, text)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText(text)
	dialog:Show(item, player)
	-- ************************** --
	
end
