--[[
	SCRIPT AI quái vật mặc định
	Tác giả: Steven Huang
	Thời gian: 18/12/2020 
]]

-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local AIMonster_Default = Scripts[100000]

-- *********************************** --
-- Danh sách kỹ năng được sử dụng
local AISkillList = {
	-- Kỹ năng cận chiến
	[1] = {
		[0] = {ID = 308, Cooldown = 0, Elemental = 0, CastRange = 80},
		[1] = {ID = 309, Cooldown = 0, Elemental = 1, CastRange = 80},
		[2] = {ID = 310, Cooldown = 0, Elemental = 2, CastRange = 80},
		[3] = {ID = 311, Cooldown = 0, Elemental = 3, CastRange = 80},
		[4] = {ID = 312, Cooldown = 0, Elemental = 4, CastRange = 80},
		[5] = {ID = 313, Cooldown = 0, Elemental = 5, CastRange = 80},
	},
	
	-- Kỹ năng tầm trung
	[2] = {
		[0] = {ID = 314, Cooldown = 11000, Elemental = 0, CastRange = 300},
		[1] = {ID = 315, Cooldown = 11000, Elemental = 1, CastRange = 300},
		[2] = {ID = 316, Cooldown = 11000, Elemental = 2, CastRange = 300},
		[3] = {ID = 317, Cooldown = 11000, Elemental = 3, CastRange = 300},
		[4] = {ID = 318, Cooldown = 11000, Elemental = 4, CastRange = 300},
		[5] = {ID = 319, Cooldown = 11000, Elemental = 5, CastRange = 300},
	},
	
	-- Kỹ năng tầm xa
	[3] = {
		[0] = {ID = 320, Cooldown = 30000, Elemental = 0, CastRange = 520},
		[1] = {ID = 321, Cooldown = 30000, Elemental = 1, CastRange = 520},
		[2] = {ID = 322, Cooldown = 30000, Elemental = 2, CastRange = 520},
		[3] = {ID = 323, Cooldown = 30000, Elemental = 3, CastRange = 520},
		[4] = {ID = 324, Cooldown = 30000, Elemental = 4, CastRange = 520},
		[5] = {ID = 325, Cooldown = 30000, Elemental = 5, CastRange = 520},
	},
}
-- *********************************** --
-- Danh sách biến cục bộ dùng cho lưu trữ
local LocalVariableID = {
	LastTickUseSkill_2 = 0,			-- Thời gian lần trước sử dụng kỹ năng tầm trung
	LastTickUseSkill_3 = 1,			-- Thời gian lần trước sử dụng kỹ năng tầm xa
}
-- *********************************** --
-- Thiết lập
local Setting = {
	LevelUseSkill2 = 30,			-- Cấp độ tối thiểu dùng kỹ năng tầm trung
	LevelUseSkill3 = 50,			-- Cấp độ tối thiểu dùng kỹ năng tầm cao
	ChanceUseSkill2 = 100,			-- Tỷ lệ % mỗi lần đến thời gian sẽ dùng kỹ năng số 2
	ChanceUseSkill3 = 100,			-- Tỷ lệ % mỗi lần đến thời gian sẽ dùng kỹ năng số 3
}
-- *********************************** --

-- ========================================================= --
--	Hàm này gọi liên tục mỗi 0.5s một lần chừng nào quái còn sống và đang đuổi theo mục tiêu
--		- Lua_Scene scene: Bản đồ hiện tại
--		- Lua_Monster monster: Quái
--		- Lua_Object target: Mục tiêu
-- ========================================================= --
function AIMonster_Default:AITick(scene, monster, target)

	-- *********************************** --
	-- Nếu lúc này không thể dùng kỹ năng
	if monster:CanUseSkillNow() == false then
		return nil
	end
	-- *********************************** --
	-- Thời gian hiện tại của hệ thống
	local nowTime = System.GetCurrentTimeMilis()
	-- *********************************** --
	-- Ngũ hành của bản thân
	local selfElement = monster:GetElemental()
	-- *********************************** --
	-- Vị trí của bản thân
	local selfPos = monster:GetPos()
	-- Vị trí của mục tiêu
	local targetPos = target:GetPos()
	-- *********************************** --
	-- Khoảng cách đến chỗ mục tiêu
	local distanceToTarget = Math.Vector2.Distance(selfPos, targetPos)
	-- *********************************** --
	local rand = math.random(1, 100)
	-- *********************************** --
	if monster:GetLevel() >= Setting.LevelUseSkill3 and AISkillList[3][selfElement] ~= nil then
		-- Thời gian dùng kỹ năng lần trước
		local lastTickUseSkill = monster:GetLocalVariable(LocalVariableID.LastTickUseSkill_3)
		-- Nếu đã hết thời gian Cooldown thì cho dùng kỹ năng
		if nowTime - lastTickUseSkill >= AISkillList[3][selfElement].Cooldown then
			-- Nếu khoảng cách phù hợp
			if distanceToTarget <= AISkillList[3][selfElement].CastRange then
				local retUseSkill = true
				-- Nếu random % đủ dùng kỹ năng này
				if rand <= Setting.ChanceUseSkill3 then
					-- Sử dụng kỹ năng
					retUseSkill = monster:UseSkill(AISkillList[3][selfElement].ID, 1, target)

					if retUseSkill == true then
						System.WriteToConsole("Use skill 3")
					end
				end
				
				-- Nếu dùng kỹ năng thành công
				if retUseSkill == true then
					-- Cập nhật lại thời gian dùng kỹ năng tương ứng
					monster:SetLocalVariable(LocalVariableID.LastTickUseSkill_3, nowTime)
				end
				
				return nil
			else
				-- Vị trí điểm cần chạy tới
				local destPos = Math.Vector2.Lerp(targetPos, selfPos, AISkillList[3][selfElement].CastRange - 20)
				-- Trả về kết quả
				return destPos
			end
		end
	end
	-- *********************************** --
	if monster:GetLevel() >= Setting.LevelUseSkill2 and AISkillList[2][selfElement] ~= nil then
		-- Thời gian dùng kỹ năng lần trước
		local lastTickUseSkill = monster:GetLocalVariable(LocalVariableID.LastTickUseSkill_2)
		-- Nếu đã hết thời gian Cooldown thì cho dùng kỹ năng
		if nowTime - lastTickUseSkill >= AISkillList[2][selfElement].Cooldown then
			-- Nếu khoảng cách phù hợp
			if distanceToTarget <= AISkillList[2][selfElement].CastRange then
				local retUseSkill = true
				-- Nếu random % đủ dùng kỹ năng này
				if rand <= Setting.ChanceUseSkill2 then
					-- Sử dụng kỹ năng
					retUseSkill = monster:UseSkill(AISkillList[2][selfElement].ID, 1, target)
					
					if retUseSkill == true then
						System.WriteToConsole("Use skill 2")
					end
				end
				
				-- Nếu dùng kỹ năng thành công
				if retUseSkill == true then
					-- Cập nhật lại thời gian dùng kỹ năng tương ứng
					monster:SetLocalVariable(LocalVariableID.LastTickUseSkill_2, nowTime)
				end
				
				return nil
			else
				-- Vị trí điểm cần chạy tới
				local destPos = Math.Vector2.Lerp(targetPos, selfPos, AISkillList[2][selfElement].CastRange - 20)
				-- Trả về kết quả
				return destPos
			end
		end
	end
	-- *********************************** --
	if AISkillList[1][selfElement] ~= nil then
		-- Nếu khoảng cách đến mục tiêu nằm trong phạm vi đánh
		if distanceToTarget <= AISkillList[1][selfElement].CastRange then
			-- Sử dụng kỹ năng
			local retUseSkill = monster:UseSkill(AISkillList[1][selfElement].ID, 1, target)
			
			-- Nếu dùng kỹ năng thành công
			if retUseSkill == true then
				System.WriteToConsole("Use skill 1")
			end
			
			return nil
		else
			-- Vị trí điểm cần chạy tới
			local destPos = Math.Vector2.Lerp(targetPos, selfPos, AISkillList[1][selfElement].CastRange - 20)
			-- Trả về kết quả
			return destPos
		end
	end
	-- *********************************** --
	-- Nếu không tìm thấy gì thì trả ra NIL
	return nil
	-- *********************************** --

end