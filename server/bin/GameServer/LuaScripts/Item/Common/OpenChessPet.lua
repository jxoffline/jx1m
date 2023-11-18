-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
--[[
--ID="1"  Name="Hoa Nam Hổ" 	      
--ID="2"  Name="Hồ Ly" 	           
--ID="3"  Name="Heo Rừng" 	        
--ID="4"  Name="Trâu Rừng" 	       
--ID="5"  Name="Kim Điêu" 	        
--ID="6"  Name="Kim Miêu" 	        
--ID="7"  Name="Hoán Hùng" 	       
--ID="8"  Name="Kim Tơ Hầu" 	 

--ID="9" Name="Bảo Bảo" 	          
--ID="14" Name="Người Tuyết" 	     
--ID="15" Name="Linh Vật" 	        
--ID="23" Name="Tiểu Ất" 	         
--ID="24" Name="Tiểu Hồng" 	       
--ID="25" Name="Tiểu Thanh" 	      
--ID="27" Name="A Lâm" 	           
--ID="28" Name="Tiểu Như" 	        
--ID="31" Name="A Hổ" 

--ID="10" Name="Tiểu Yêu Tinh" 	   
--ID="12" Name="Tiểu Hồ Lô" 	      
--ID="18" Name="Bích Lân Hoàng" 	  
--ID="20" Name="Tế Công" 	         
--ID="22" Name="Đại Hùng" 	        
--ID="26" Name="Thần Hầu Vương" 	  
--ID="29" Name="Bạch Thiếu Chủ" 	  
--ID="30" Name="Bạch Tiểu Thư"

--ID="11" Name="Hỏa Kỳ Lân" 	      
--ID="13" Name="Hồng Ảnh" 	        
--ID="16" Name="Kim Ảnh" 	         
--ID="17" Name="Thanh Sát" 	       
--ID="19" Name="Hoa Tiên Tử" 	     
--ID="21" Name="Tinh Điệp" 	       
--ID="32" Name="Mộng Hằng Chi Diệp"
--ID="34" Name="Hỏa Song" 	        
--ID="33" Name="Tử Kiêu" 	       
]]
local OpenChessPet = Scripts[200100]

-- ************************** --
local PetList = { -- Danh sách ID trứng
    [12447] = {
        PetID = 9, --ID="9" Name="Bảo Bảo"

        ShowNotification = false,
    },
    [12448] = {
        PetID = 14, --ID="14" Name="Người Tuyết"

        ShowNotification = false,
    },
    [12449] = {
        PetID = 15,
        ShowNotification = false, --ID="15" Name="Linh Vật"

    },
    [12450] = {
        PetID = 23, --ID="23" Name="Tiểu Ất"

        ShowNotification = false,
    },
    [12451] = {
        PetID = 24, --ID="24" Name="Tiểu Hồng"

        ShowNotification = false,
    },
    [12452] = {
        PetID = 25,
        ShowNotification = false --ID="25" Name="Tiểu Thanh"

    },
    [12453] = {
        PetID = 27, --ID="27" Name="A Lâm"

        ShowNotification = false,
    },
    [12454] = {
        PetID = 28, --ID="28" Name="Tiểu Như"

        ShowNotification = false,
    },
    [12455] = {
        PetID = 31, --ID="31" Name="A Hổ" ,
        ShowNotification = false,
    },
    [12456] = {
        PetID = 10, --ID="10" Name="Tiểu Yêu Tinh"

        ShowNotification = true,
    },
    [12457] = {
        PetID = 12, --ID="12" Name="Tiểu Hồ Lô"

        ShowNotification = true,
    },
    [12458] = {
        PetID = 18, --ID="18" Name="Bích Lân Hoàng"

        ShowNotification = true,
    },
    [12459] = {
        PetID = 20, --ID="20" Name="Tế Công"

        ShowNotification = true,
    },
    [12460] = {
        PetID = 22, --ID="22" Name="Đại Hùng"

        ShowNotification = true,
    },
    [12461] = {
        PetID = 26, --ID="26" Name="Thần Hầu Vương"

        ShowNotification = true,
    },
    [12462] = {
        PetID = 29, --ID="29" Name="Bạch Thiếu Chủ"

        ShowNotification = true,
    },
    [12463] = {
        PetID = 30, --ID="30" Name="Bạch Tiểu Thư"
        ShowNotification = true,
    },
    [12465] = {
        PetID = 11, --ID="11" Name="Hỏa Kỳ Lân" 	      
       
      ShowNotification = true,
    },
    [12466] = {
        PetID = 13,  --ID="13" Name="Hồng Ảnh" 	        
     
     ShowNotification = true,
    },
    [12467] = {
        PetID = 16,    --ID="16" Name="Kim Ảnh" 	         
        
       ShowNotification = true,
    },
    [12468] = {
        PetID = 17, --ID="17" Name="Thanh Sát" 	       
      
         ShowNotification = true,
    },
    [12469] = {
        PetID = 19,   --ID="19" Name="Hoa Tiên Tử" 	     
        
       ShowNotification = true,
    },
    [12470] = {
        PetID = 21, --ID="21" Name="Tinh Điệp" 	       
       
        ShowNotification = true,
    },
    [12471] = {
        PetID = 32,  --ID="32" Name="Mộng Hằng Chi Diệp"
       
      ShowNotification = true,
    },
    [12472] = {
        PetID = 34,  --ID="34" Name="Hỏa Song" 	        
      
    ShowNotification = true,
    },
    [12473] = {
        PetID = 33 ,   --ID="33" Name="Tử Kiêu" 
    ShowNotification = true,
    },
    [12480] = {
        PetID = 1, --ID="1"  Name="Hoa Nam Hổ"

        ShowNotification = false,
    },
    [12481] = {
        PetID = 2, --ID="2"  Name="Hồ Ly"

        ShowNotification = false,
    },
    [12482] = {
        PetID = 3, --ID="3"  Name="Heo Rừng"

        ShowNotification = false,
    },
    [12483] = {
        PetID = 4, --ID="4"  Name="Trâu Rừng"

        ShowNotification = false,
    },
    [12484] = {
        PetID = 5, --ID="5"  Name="Kim Điêu"

        ShowNotification = false,
    },
    [12485] = {
        PetID = 6, --ID="6"  Name="Kim Miêu"

        ShowNotification = false,
    },
    [12486] = {
        PetID = 7, --ID="7"  Name="Hoán Hùng"
        ShowNotification = false,
    },
    [12487] = {
        PetID = 8, --ID="8"  Name="Kim Tơ Hầu"
        ShowNotification = false,
    },

}
-- ************************** --

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function OpenChessPet:OnPreCheckCondition(scene, item, player, otherParams)

    -- ************************** --
    -- Nếu vật phẩm không tồn tại
    if PetList[item:GetItemID()] == nil then
        player:AddNotification("Vật phẩm không hợp lệ!" .. item:GetItemID() .. "")
        return false
    end

    -- Nếu đã quá số lượng pet mang theo
    if player:GetTotalPets() >= Player.GetMaxPetCanTake() then
        player:AddNotification(string.format("Số lượng tinh linh mang theo không thể vượt quá %d!",
            Player.GetMaxPetCanTake()))
        return false
    end

    -- OK
    return true
    -- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi để thực thi Logic khi người sử dụng vật phẩm, sau khi đã thỏa mãn hàm kiểm tra điều kiện
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function OpenChessPet:OnUse(scene, item, player, otherParams)

    -- ************************** --
    -- Nếu vật phẩm không tồn tại
    if PetList[item:GetItemID()] == nil then
        player:AddNotification("Vật phẩm không hợp lệ!")
        return
    end

    -- Nếu đã quá số lượng pet mang theo
    if player:GetTotalPets() >= Player.GetMaxPetCanTake() then
        player:AddNotification(string.format("Số lượng tinh linh mang theo không thể vượt quá %d!",
            Player.GetMaxPetCanTake()))
        return
    end

    -- Thông tin trứng
    local data = PetList[item:GetItemID()]

    -- Thực hiện tạo Pet tương ứng
    local petID = Player.CreatePet(player, data.PetID)
    player:AddNotification("Ngươi đã mở " .. item:GetName() .. " thành công!")
    -- Toác
    if petID == -1 then
        return
    end

    -- Thực hiện xóa vật phẩm
    item:FinishUsing(player)

    -- Nếu có thông báo kênh hệ thống
    if data.ShowNotification == true then
        -- Thông tin vật phẩm
        local itemDescString = GUI.GetItemInfoString(item)
        -- Thông tin pet
        local petDescString = GUI.GetPetInfoString(player, petID)
        -- Thông báo kênh hệ thống
        GUI.SendSystemMessage(string.format("Người chơi <color=#7bafea>[%s]</color> mở thành công %s và nhận được %s."
            , player:GetName(), itemDescString, petDescString), { item }, player, { petID })
    end
    -- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function OpenChessPet:OnSelection(scene, item, player, selectionID, otherParams)

    -- ************************** --
    -- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function OpenChessPet:OnItemSelected(scene, item, player, itemID)

    -- ************************** --
    -- ************************** --

end
